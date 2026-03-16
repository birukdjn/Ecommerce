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
using Npgsql.Replication.PgOutput.Messages;
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
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();

        // string? confirmEmailEndpointName = null;

        var routeGroup = endpoints.MapGroup("");

        // POST: /register
        routeGroup.MapPost("/register", async Task<Results<Ok<string>, ValidationProblem>>
            ([FromBody] RegisterCommand command, HttpContext context, [FromServices] IServiceProvider serviceProvider) =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<TUser>>();
            var smsSender = serviceProvider.GetRequiredService<ISmsSender>();



            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapCustomIdentityApi)} requires a user store with email support.");
            }

            var userStore = serviceProvider.GetRequiredService<IUserStore<TUser>>();
            var emailStore = (IUserEmailStore<TUser>)userStore;

            var email = command.Email;
            var phone = command.PhoneNumber;
            var fullName = command.FullName;
            var profilePicture = command.ProfilePictureUrl;
            var password = command.Password;


            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
            }

            if (await userManager.FindByEmailAsync(email) is not null)
            {
                return CreateValidationProblem("DuplicateEmail", "This email address is already registered.");
            }

            var existingPhone = await userManager.Users.AnyAsync(u =>
                EF.Property<string>(u, "PhoneNumber") == phone);
            if (existingPhone)
            {
                return CreateValidationProblem("DuplicatePhone", "This phone number is already in use.");
            }

            var user = new TUser();
            await userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);

            if (user is ApplicationUser customUser)
            {
                customUser.FullName = fullName;
                customUser.ProfilePictureUrl = profilePicture;
            }
            await userManager.SetPhoneNumberAsync(user, phone);
            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            await userManager.AddToRoleAsync(user, Roles.Customer);

            // await SendConfirmationEmailAsync(user, userManager, context, email);

            var addedPhoneNumber = await userManager.GetPhoneNumberAsync(user);

            if (!string.IsNullOrEmpty(addedPhoneNumber))
            {
                var smsResult = await smsSender.SendSmsChallengeAsync(addedPhoneNumber);
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
            ([FromBody] LoginCommand command, [FromServices] IServiceProvider sp) =>
        {

            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            var signInManager = sp.GetRequiredService<SignInManager<TUser>>();

            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

            TUser? user = null;

            if (_emailAddressAttribute.IsValid(command.PhoneOrEmail))
            {
                user = await userManager.FindByEmailAsync(command.PhoneOrEmail);
            }

            user ??= await userManager.Users.FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "PhoneNumber") == command.PhoneOrEmail);
            if (user == null)
            {
                return TypedResults.Problem("Invalid login attempt.", statusCode: StatusCodes.Status401Unauthorized);
            }
            var result = await signInManager.CheckPasswordSignInAsync(
                         user,
                         command.Password,
                         lockoutOnFailure: true);

            /*
                if (result.RequiresTwoFactor)
                {
                    if (!string.IsNullOrEmpty(command.TwoFactorCode))
                    {
                        result = await signInManager.TwoFactorAuthenticatorSignInAsync
                        (
                            command.TwoFactorCode, 
                            command.RememberMe,
                            rememberClient: command.RememberMe);
                    }
                    else if (!string.IsNullOrEmpty(command.TwoFactorRecoveryCode))
                    {
                        result = await signInManager.TwoFactorRecoveryCodeSignInAsync(command.TwoFactorRecoveryCode);
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
            (VerifyPhoneCommand command, [FromServices] IServiceProvider serviceProvider) =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<TUser>>();
            var smsSender = serviceProvider.GetRequiredService<ISmsSender>();
            var phone = command.PhoneNumber;
            var code = command.Code;
            var verificationId = command.VerificationId;

            // 1. Verify code with AfroSms
            var verificationResult = await smsSender.VerifyCodeAsync(phone, code, verificationId);

            if (!verificationResult.IsSuccess)
            {
                return CreateValidationProblem("InvalidCode", "The verification code is incorrect or expired.");
            }

            // 2. Update User in Database
            var user = await userManager.Users.FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "PhoneNumber") == phone);

            if (user == null) return TypedResults.NotFound();

            // Mark as confirmed
            var token = await userManager.GenerateChangePhoneNumberTokenAsync(user, phone);
            var identityResult = await userManager.ChangePhoneNumberAsync(user, phone, token);

            if (identityResult.Succeeded && user is ApplicationUser customUser)
            {
                var jobService = serviceProvider.GetRequiredService<IJobService>();
                jobService.Enqueue<ISmsSender>(sms => sms.SendSmsAsync(phone, "Welcome to our platform! Your account is now active."));

                customUser.IsActive = true;
                await userManager.UpdateAsync(user);
                return TypedResults.Ok();

            }
            return identityResult.Succeeded
                ? TypedResults.Ok()
                : CreateValidationProblem(identityResult);
        });


        // POST: /forgotPassword
        routeGroup.MapPost("/forgot-password", async Task<Results<Ok<object>, ValidationProblem>>
            ([FromBody] ForgotPasswordCommand command, [FromServices] IServiceProvider serviceProvider) =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<TUser>>();
            var jobService = serviceProvider.GetRequiredService<IJobService>();


            var phone = command.PhoneNumber;
            var user = await userManager.Users.FirstOrDefaultAsync(u =>
                 EF.Property<string>(u, "PhoneNumber") == phone);

            if (user is not null && await userManager.IsPhoneNumberConfirmedAsync(user))
            {
                var code = await userManager.GenerateTwoFactorTokenAsync(user, "Phone");

                jobService.Enqueue<ISmsSender>(sms => sms.SendSmsAsync(phone, $"Your password reset code is: {code}"));

            }

            return TypedResults.Ok((object)new
            {
                Message = "If an account is associated with this phone number, a reset code has been sent."
            });
        });

        // POST: /resetPassword
        routeGroup.MapPost("/reset-password", async Task<Results<Ok<object>, ValidationProblem>>
            ([FromBody] ResetPasswordCommand command, [FromServices] IServiceProvider serviceProvider) =>
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<TUser>>();

            var phone = command.PhoneNumber;
            var resetCode = command.ResetCode;
            var newPassword = command.NewPassword;

            var user = await userManager.Users.FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "PhoneNumber") == phone);

            if (user is null || !await userManager.IsPhoneNumberConfirmedAsync(user))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
            }

            var isValid = await userManager.VerifyTwoFactorTokenAsync(user, "Phone", resetCode);

            if (!isValid)
            {
                return CreateValidationProblem("InvalidCode", "The 6-digit code is incorrect.");
            }

            var internaltoken = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, internaltoken, newPassword);


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