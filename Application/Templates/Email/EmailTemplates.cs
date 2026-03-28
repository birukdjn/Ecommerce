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

        public static string GetRegistrationEmail(string name) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: {PrimaryColor};'>Welcome, {name}!</h2>
                <p>Thank you for joining our platform. Your account is not active.</p>
                <p>please check you phone's inbox and verify your account!</p>
                <div style='{FooterStyle}'>
                    If you didn't sign up for this account, please don't share your phone sms message.
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

        // 5. Product Approval Notification for Vendor
        public static string GetProductApprovedEmail(string vendorName, string productName) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #28a745;'>Product Approved!</h2>
                <p>Hello {vendorName},</p>
                <p>Great news! Your product <strong>{productName}</strong> has been reviewed and approved by our administrators.</p>
                <p>It is now live on the storefront and available for customers to purchase.</p>
                <div style='{FooterStyle}'>
                    Thank you for partnering with us.
                </div>
            </div>";

        // 6. Product Rejected Notification for Vendor
        public static string GetProductRejectedEmail(string vendorName, string productName, string reason) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #dc3545;'>Product Update: Action Required</h2>
                <p>Hello {vendorName},</p>
                <p>Your product <strong>{productName}</strong> was reviewed by our team and, unfortunately, it could not be approved at this time.</p>
                <div style='background-color: #fff5f5; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0;'>
                    <strong>Reason for Rejection:</strong><br/>
                    {reason}
                </div>
                <p>Please update the product details based on the feedback above and resubmit it for approval.</p>
                <div style='{FooterStyle}'>
                    If you have questions, please reply to this email.
                </div>
            </div>";

        // 8. Product Submission Confirmation
        public static string GetProductSubmittedEmail(string vendorName, string productName) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: {PrimaryColor};'>Product Received!</h2>
                <p>Hello {vendorName},</p>
                <p>Your product <strong>{productName}</strong> has been successfully added to your catalog.</p>
                <div style='background-color: #e7f3ff; border-left: 4px solid {PrimaryColor}; padding: 15px; margin: 20px 0;'>
                    <strong>Status: Pending Approval</strong><br/>
                    Our administrators will review the details shortly. You will receive another notification once it is live on the store.
                </div>
                <p>In the meantime, you can continue managing your other products from your dashboard.</p>
                <div style='{FooterStyle}'>
                    Thank you for selling with us!
                </div>
            </div>";

        // 9. Low Stock Alert
        public static string GetLowStockEmail(string vendorName, string productName, int currentStock) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #e67e22;'>Low Stock Warning</h2>
                <p>Hello {vendorName},</p>
                <p>This is an automated alert to inform you that your inventory for <strong>{productName}</strong> is running low.</p>
                <div style='background-color: #fff4e5; border: 1px solid #e67e22; padding: 15px; text-align: center; border-radius: 4px;'>
                    <span style='font-size: 18px;'>Current Stock: <strong>{currentStock}</strong></span>
                </div>
                <p>To avoid losing potential sales, please restock this item as soon as possible.</p>
                <div style='{FooterStyle}'>
                    You can update your stock levels in the Vendor Portal.
                </div>
            </div>";

        // 10. New Sale Alert (For Vendor)
        public static string GetNewSaleAlertEmail(string vendorName, string orderNumber, decimal amount) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #28a745;'>You made a sale!</h2>
                <p>Hello {vendorName},</p>
                <p>A customer has just purchased items from your store. Order <strong>#{orderNumber}</strong> is ready for processing.</p>
                <div style='background-color: #f9f9f9; padding: 15px; border: 1px solid #ddd;'>
                    <strong>Amount to be credited:</strong> ${amount:N2}
                </div>
                <p>Please log in to your dashboard to view shipping details.</p>
            </div>";

        // 11. Payout Processed (For Vendor)
        public static string GetPayoutProcessedEmail(string vendorName, decimal amount) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: {PrimaryColor};'>Funds Dispatched</h2>
                <p>Hi {vendorName},</p>
                <p>We've processed your payout of <strong>${amount:N2}</strong>. The funds should appear in your account within 3-5 business days.</p>
            </div>";

        // 12. Payout Requested (For Admin)
        public static string GetPayoutRequestedAdminEmail(string vendorName, decimal amount) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: {PrimaryColor};'>New Payout Request</h2>
                <p>Vendor <strong>{vendorName}</strong> has requested a payout of <strong>${amount:N2}</strong>.</p>
                <p>Please review the vendor's wallet and transaction history in the Admin Finance panel.</p>
            </div>";

        // 13. Order Shipped (For Customer)
        public static string GetOrderShippedEmail(string customerName, string orderNumber, string vendorName) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: {PrimaryColor};'>Your Order is on the Way!</h2>
                <p>Hi {customerName},</p>
                <p>Great news! <strong>{vendorName}</strong> has shipped your items from Order <strong>#{orderNumber}</strong>.</p>
                <p>You can track your package details in your account dashboard.</p>
            </div>";

        // 14. Refund Processed (For Customer)
        public static string GetRefundProcessedEmail(string name, string orderNumber, decimal amount) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #6c757d;'>Refund Processed</h2>
                <p>Hi {name},</p>
                <p>A refund of <strong>${amount:N2}</strong> for Order <strong>#{orderNumber}</strong> has been successfully processed.</p>
                <p>The funds should appear back on your original payment method within 5-10 business days.</p>
            </div>";

        // 16. Refund Notification (For Customer)
        public static string GetRefundEmail(string name, string orderNumber, decimal amount, string status) => $@"
            <div style='{ContainerStyle}'>
                <h2 style='color: #6c757d;'>Refund Update: {status}</h2>
                <p>Hi {name},</p>
                <p>The refund for Order <strong>#{orderNumber}</strong> in the amount of <strong>${amount:N2}</strong> has been {status.ToLower()}.</p>
                <div style='{FooterStyle}'>It may take several days to appear on your bank statement.</div>
            </div>";

        // 17. New Vendor Application (For Admin)
        public static string GetNewVendorApplicationEmail(string storeName, string vendorName) => $@"
    <div style='{ContainerStyle}'>
        <h2 style='color: {PrimaryColor};'>New Vendor Application</h2>
        <p>A new vendor, <strong>{storeName}</strong>, has applied to join the platform.</p>
        <p><strong>Applicant:</strong> {vendorName}</p>
        <p>Please log in to the Admin Portal to review their documents and approve or reject the request.</p>
    </div>";

        // 18. Vendor Approval (For Vendor)
        public static string GetVendorApprovedEmail(string vendorName, string storeName) => $@"
    <div style='{ContainerStyle}'>
        <h2 style='color: #28a745;'>Welcome to the Marketplace!</h2>
        <p>Hi {vendorName},</p>
        <p>Your application for <strong>{storeName}</strong> has been approved!</p>
        <p>You can now start adding products to your catalog and selling to customers.</p>
        <div style='margin-top: 20px; text-align: center;'>
            <a href='#' style='background: {PrimaryColor}; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Go to Vendor Dashboard</a>
        </div>
    </div>";

        // 19. Payout Success (For Vendor)
        public static string GetPayoutSuccessEmail(string vendorName, decimal amount, string bankAccount) => $@"
    <div style='{ContainerStyle}'>
        <h2 style='color: #28a745;'>Payout Processed</h2>
        <p>Hello {vendorName},</p>
        <p>We've successfully processed your payout of <strong>${amount:N2}</strong>.</p>
        <p><strong>Destination:</strong> {bankAccount}</p>
        <p>The funds should appear in your account within 3 to 5 business days.</p>
    </div>";
    }
}