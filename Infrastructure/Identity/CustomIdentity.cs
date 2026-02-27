using Application.DTOs;
using Application.Features.Users.Commands.Identity;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Extensions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Infrastructure.Identity;

public static class IdentityApiEndpointRouteBuilderExtensions
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    public static IEndpointConventionBuilder MapCustomIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var timeProvider = endpoints.ServiceProvider.GetRequiredService<TimeProvider>();
        var bearerTokenOptions = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
        var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<TUser>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();

        // string? confirmEmailEndpointName = null;

        var routeGroup = endpoints.MapGroup("");

        // POST: /register
        routeGroup.MapPost("/register", async Task<Results<Ok<string>, ValidationProblem>>
            ([FromBody] RegisterCommand registration, HttpContext context, [FromServices] IServiceProvider serviceProvider) =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<TUser>>();
            var sms = serviceProvider.GetRequiredService<ISmsSender>();

            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapCustomIdentityApi)} requires a user store with email support.");
            }

            var userStore = serviceProvider.GetRequiredService<IUserStore<TUser>>();
            var emailStore = (IUserEmailStore<TUser>)userStore;
            var email = registration.Email;

            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
            }

            var user = new TUser();
            await userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);

            if (user is ApplicationUser customUser)
            {
                customUser.FullName = registration.FullName;
                customUser.ProfilePictureUrl = registration.ProfilePictureUrl;
            }
            await userManager.SetPhoneNumberAsync(user, registration.PhoneNumber);
            var result = await userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            await userManager.AddToRoleAsync(user, Roles.Customer);

            // await SendConfirmationEmailAsync(user, userManager, context, email);

            var phoneNumber = await userManager.GetPhoneNumberAsync(user);

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var smsResult = await sms.SendSmsChallengeAsync(phoneNumber);
                if (smsResult.IsSuccess)
                {
                    return TypedResults.Ok(smsResult.Value);
                }

                return CreateValidationProblem("SmsError", smsResult.Error ?? "Failed to send verification SMS.");
            }
            return TypedResults.Ok(string.Empty);
        });

        // POST: /login
        routeGroup.MapPost("/login", async Task<Results<SignInHttpResult, ProblemHttpResult>>
            ([FromBody] LoginCommand login, [FromServices] IServiceProvider sp) =>
        {

            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            var signInManager = sp.GetRequiredService<SignInManager<TUser>>();

            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

            TUser? user = null;

            if (_emailAddressAttribute.IsValid(login.PhoneOrEmail))
            {
                user = await userManager.FindByEmailAsync(login.PhoneOrEmail);
            }

            user ??= await userManager.Users.FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "PhoneNumber") == login.PhoneOrEmail);
            if (user == null)
            {
                return TypedResults.Problem("Invalid login attempt.", statusCode: StatusCodes.Status401Unauthorized);
            }
            var result = await signInManager.CheckPasswordSignInAsync(
                         user,
                         login.Password,
                         lockoutOnFailure: true);

            /*
                if (result.RequiresTwoFactor)
                {
                    if (!string.IsNullOrEmpty(login.TwoFactorCode))
                    {
                        result = await signInManager.TwoFactorAuthenticatorSignInAsync
                        (
                            login.TwoFactorCode, 
                            login.RememberMe,
                            rememberClient: login.RememberMe);
                    }
                    else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                    {
                        result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                    }
                }
            */

            if (!await userManager.IsPhoneNumberConfirmedAsync(user))
            {
                return TypedResults.Problem("Please verify your phone number before logging in.", statusCode: StatusCodes.Status403Forbidden);
            }

            if (!result.Succeeded)
            {
                return TypedResults.Problem("Invalid login attempt.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var principal = await signInManager.CreateUserPrincipalAsync(user);
            return TypedResults.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
        });

        // POST: /refresh
        routeGroup.MapPost("/refresh", async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>>
            ([FromBody] RefreshRequest refreshRequest, [FromServices] IServiceProvider sp) =>
        {
            var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
            var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                timeProvider.GetUtcNow() >= expiresUtc ||
                await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not TUser user)

            {
                return TypedResults.Challenge();
            }

            var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
        });

        // POST: /verify-phone
        routeGroup.MapPost("/verify-phone", async Task<Results<Ok, ValidationProblem, NotFound>>
            (string phoneNumber, string code, string verificationId, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            var smsSender = sp.GetRequiredService<ISmsSender>();

            // 1. Verify code with AfroSms
            var verificationResult = await smsSender.VerifyCodeAsync(phoneNumber, code, verificationId);

            if (!verificationResult.IsSuccess)
            {
                return CreateValidationProblem("InvalidCode", "The verification code is incorrect or expired.");
            }

            // 2. Update User in Database
            var user = await userManager.Users.FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "PhoneNumber") == phoneNumber);

            if (user == null) return TypedResults.NotFound();

            // Mark as confirmed
            var token = await userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            var identityResult = await userManager.ChangePhoneNumberAsync(user, phoneNumber, token);

            return identityResult.Succeeded
                ? TypedResults.Ok()
                : CreateValidationProblem(identityResult);
        });


        // POST: /forgotPassword
        routeGroup.MapPost("/forgotPassword", async Task<Results<Ok<object>, ValidationProblem>>
            ([FromBody] ForgotPasswordRequestDto resetRequest, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            var smsSender = sp.GetRequiredService<ISmsSender>();
            var user = await userManager.Users.FirstOrDefaultAsync(u =>
                 EF.Property<string>(u, "PhoneNumber") == resetRequest.PhoneNumber);

            if (user is not null && await userManager.IsPhoneNumberConfirmedAsync(user))
            {
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                bool isSent = await smsSender.SendSmsAsync(
                    resetRequest.PhoneNumber,
                    $"Your password reset code is: {encodedCode}");

                if (!isSent)
                {
                    return CreateValidationProblem("SmsError", "Failed to send reset code.");
                }

            }

            return TypedResults.Ok((object)new
            {
                Message = "If an account is associated with this phone number, a reset code has been sent."
            });
        });

        // POST: /resetPassword
        routeGroup.MapPost("/resetPassword", async Task<Results<Ok<object>, ValidationProblem>>
            ([FromBody] ResetPasswordRequestByPhone resetRequest, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();

            var user = await userManager.Users.FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "PhoneNumber") == resetRequest.PhoneNumber);

            if (user is null || !(await userManager.IsPhoneNumberConfirmedAsync(user)))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
            }

            IdentityResult result;
            try
            {
                var DecodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                result = await userManager.ResetPasswordAsync(user, DecodedCode, resetRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
            }

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            return TypedResults.Ok((object)new
            {
                Message = "Your password has been reset successfully. You can now log in with your new password."
            });
        });


        /*
                // GET: /confirmEmail
                routeGroup.MapGet("/confirmEmail", async Task<Results<ContentHttpResult, UnauthorizedHttpResult>>
                    ([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail, [FromServices] IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<TUser>>();
                    if (await userManager.FindByIdAsync(userId) is not { } user)
                    {
                        return TypedResults.Unauthorized();
                    }

                    try
                    {
                        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                    }
                    catch (FormatException)
                    {
                        return TypedResults.Unauthorized();
                    }

                    IdentityResult result;

                    if (string.IsNullOrEmpty(changedEmail))
                    {
                        result = await userManager.ConfirmEmailAsync(user, code);
                    }
                    else
                    {
                        result = await userManager.ChangeEmailAsync(user, changedEmail, code);

                        if (result.Succeeded)
                        {
                            result = await userManager.SetUserNameAsync(user, changedEmail);
                        }
                    }

                    if (!result.Succeeded)
                    {
                        return TypedResults.Unauthorized();
                    }

                    return TypedResults.Text("Thank you for confirming your email.");
                }).Add(endpointBuilder =>
                {
                    var finalPattern = ((RouteEndpointBuilder)endpointBuilder).RoutePattern.RawText;
                    confirmEmailEndpointName = $"{nameof(MapCustomIdentityApi)}-{finalPattern}";
                    endpointBuilder.Metadata.Add(new EndpointNameMetadata(confirmEmailEndpointName));
                });

                // POST: /resendConfirmationEmail
                routeGroup.MapPost("/resendConfirmationEmail", async Task<Ok>
                    ([FromBody] ResendConfirmationEmailRequest resendRequest, HttpContext context, [FromServices] IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<TUser>>();
                    if (await userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
                    {
                        return TypedResults.Ok();
                    }

                    await SendConfirmationEmailAsync(user, userManager, context, resendRequest.Email);
                    return TypedResults.Ok();
                });

                var accountGroup = routeGroup.MapGroup("/manage").RequireAuthorization();

                // POST: /manage/2fa
                accountGroup.MapPost("/2fa", async Task<Results<Ok<TwoFactorResponse>, ValidationProblem, NotFound>>
                    (ClaimsPrincipal claimsPrincipal, [FromBody] TwoFactorRequest tfaRequest, [FromServices] IServiceProvider sp) =>
                {
                    var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
                    var userManager = signInManager.UserManager;
                    if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
                    {
                        return TypedResults.NotFound();
                    }

                    if (tfaRequest.Enable == true)
                    {
                        if (tfaRequest.ResetSharedKey)
                        {
                            return CreateValidationProblem("CannotResetSharedKeyAndEnable",
                                "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
                        }

                        if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
                        {
                            return CreateValidationProblem("RequiresTwoFactor",
                                "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
                        }

                        if (!await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, tfaRequest.TwoFactorCode))
                        {
                            return CreateValidationProblem("InvalidTwoFactorCode",
                                "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
                        }

                        await userManager.SetTwoFactorEnabledAsync(user, true);
                    }
                    else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
                    {
                        await userManager.SetTwoFactorEnabledAsync(user, false);
                    }

                    if (tfaRequest.ResetSharedKey)
                    {
                        await userManager.ResetAuthenticatorKeyAsync(user);
                    }

                    string[]? recoveryCodes = null;
                    if (tfaRequest.ResetRecoveryCodes || (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
                    {
                        var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                        recoveryCodes = recoveryCodesEnumerable?.ToArray();
                    }

                    if (tfaRequest.ForgetMachine)
                    {
                        await signInManager.ForgetTwoFactorClientAsync();
                    }

                    var key = await userManager.GetAuthenticatorKeyAsync(user);
                    if (string.IsNullOrEmpty(key))
                    {
                        await userManager.ResetAuthenticatorKeyAsync(user);
                        key = await userManager.GetAuthenticatorKeyAsync(user);

                        if (string.IsNullOrEmpty(key))
                        {
                            throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
                        }
                    }

                    return TypedResults.Ok(new TwoFactorResponse
                    {
                        SharedKey = key,
                        RecoveryCodes = recoveryCodes,
                        RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
                        IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
                        IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
                    });
                });

                // GET: /manage/info
                accountGroup.MapGet("/info", async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>>
                    (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<TUser>>();
                    if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
                    {
                        return TypedResults.NotFound();
                    }

                    return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
                });

                // POST: /manage/info
                accountGroup.MapPost("/info", async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>>
                    (ClaimsPrincipal claimsPrincipal, [FromBody] InfoRequest infoRequest, HttpContext context, [FromServices] IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<TUser>>();
                    if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
                    {
                        return TypedResults.NotFound();
                    }

                    if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !_emailAddressAttribute.IsValid(infoRequest.NewEmail))
                    {
                        return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
                    }

                    if (!string.IsNullOrEmpty(infoRequest.NewPassword))
                    {
                        if (string.IsNullOrEmpty(infoRequest.OldPassword))
                        {
                            return CreateValidationProblem("OldPasswordRequired",
                                "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");
                        }

                        var changePasswordResult = await userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
                        if (!changePasswordResult.Succeeded)
                        {
                            return CreateValidationProblem(changePasswordResult);
                        }
                    }

                    if (!string.IsNullOrEmpty(infoRequest.NewEmail))
                    {
                        var email = await userManager.GetEmailAsync(user);

                        if (email != infoRequest.NewEmail)
                        {
                            await SendConfirmationEmailAsync(user, userManager, context, infoRequest.NewEmail, isChange: true);
                        }
                    }

                    return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
                });


                async Task SendConfirmationEmailAsync(TUser user, UserManager<TUser> userManager, HttpContext context, string email, bool isChange = false)
                {
                    if (confirmEmailEndpointName is null)
                    {
                        throw new NotSupportedException("No email confirmation endpoint was registered!");
                    }

                    var code = isChange
                        ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                        : await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var userId = await userManager.GetUserIdAsync(user);
                    var routeValues = new RouteValueDictionary()
                    {
                        ["userId"] = userId,
                        ["code"] = code,
                    };

                    if (isChange)
                    {
                        routeValues.Add("changedEmail", email);
                    }

                    var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
                        ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

                    await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
                }

        */

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }


    private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]> { { errorCode, [errorDescription] } });

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        Debug.Assert(!result.Succeeded);

        var domainResult = result.ToResult();

        return TypedResults.ValidationProblem(new Dictionary<string, string[]> {
            { "IdentityError", [domainResult.Error ?? "An unknown identity error occurred."] }
        });
    }

    private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
        where TUser : class
    {
        return new()
        {
            Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
        };
    }

    private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;
        public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
        public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromBodyAttribute : Attribute, IFromBodyMetadata { }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromServicesAttribute : Attribute, IFromServiceMetadata { }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromQueryAttribute : Attribute, IFromQueryMetadata { public string? Name => null; }
}