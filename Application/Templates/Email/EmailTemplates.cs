namespace Application.Templates.Email
{
    public static class EmailTemplates
    {
        private const string PrimaryColor = "#2d89ef";
        private const string FooterStyle = "font-size: 12px; color: #777; margin-top: 20px; border-top: 1px solid #eee; padding-top: 10px;";
        private const string ContainerStyle = "font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;";

        // 1. Welcome Email
        public static string GetWelcomeEmail(string name) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: {PrimaryColor};'>Welcome, {name}!</h2>
                <p>Thank you for joining our platform. Your account is now active and ready for shopping.</p>
                <p>Enjoy our latest collections and exclusive offers!</p>
                <div style='{FooterStyle}'>
                    If you didn't sign up for this account, please ignore this email.
                </div>
            </div>";

        // 2. Password Reset Code
        public static string GetPasswordResetEmail(string name, string code) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: {PrimaryColor};'>Reset Your Password</h2>
                <p>Hello {name},</p>
                <p>We received a request to reset your password. Use the verification code below to proceed:</p>
                <div style='background: #f4f4f4; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; border-radius: 4px;'>
                    {code}
                </div>
                <p>This code will expire in 15 minutes.</p>
                <div style='{FooterStyle}'>
                    If you did not request a password reset, please secure your account.
                </div>
            </div>";

        // 3. Order Confirmation
        public static string GetOrderConfirmationEmail(string name, string orderNumber, decimal totalAmount) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #28a745;'>Order Confirmed!</h2>
                <p>Hi {name},</p>
                <p>Thank you for your purchase. We are preparing your order for shipment.</p>
                <div style='border: 1px solid #eee; padding: 15px; background-color: #f9f9f9;'>
                    <strong>Order Number:</strong> #{orderNumber}<br/>
                    <strong>Total Amount:</strong> ${totalAmount:N2}
                </div>
                <p>You will receive another email with a tracking number once your package ships.</p>
                <div style='{FooterStyle}'>
                    Thank you for choosing our store!
                </div>
            </div>";

        // 4. Login Notification (Security Alert)
        public static string GetLoginNotificationEmail(string name, string device, string location, string dateTime) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #dc3545;'>New Login Detected</h2>
                <p>Hi {name},</p>
                <p>We noticed a new login to your account with the following details:</p>
                <ul>
                    <li><strong>Time:</strong> {dateTime}</li>
                    <li><strong>Device:</strong> {device}</li>
                    <li><strong>Location:</strong> {location}</li>
                </ul>
                <p>If this was you, you can safely ignore this email. If not, please reset your password immediately.</p>
                <div style='{FooterStyle}'>
                    This is an automated security notification.
                </div>
            </div>";
    }
}