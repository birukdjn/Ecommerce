namespace Application.DTOs
{
    public record ResetPasswordRequestByPhone(
    string PhoneNumber,
    string ResetCode,
    string NewPassword
);
}