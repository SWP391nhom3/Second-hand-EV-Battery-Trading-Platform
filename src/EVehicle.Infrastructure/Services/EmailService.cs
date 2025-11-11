using System.Net;
using System.Net.Mail;
using EVehicle.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EVehicle.Infrastructure.Services;

/// <summary>
/// Email Service implementation sử dụng SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly bool _enableSsl;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var smtpSection = _configuration.GetSection("MailSettings:Smtp");
        _smtpHost = smtpSection["Host"] ?? throw new InvalidOperationException("MailSettings:Smtp:Host chưa được cấu hình");
        _smtpPort = int.Parse(smtpSection["Port"] ?? "587");
        _enableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "true");
        _smtpUsername = smtpSection["Username"] ?? throw new InvalidOperationException("MailSettings:Smtp:Username chưa được cấu hình");
        _smtpPassword = smtpSection["Password"] ?? throw new InvalidOperationException("MailSettings:Smtp:Password chưa được cấu hình");
        _fromEmail = smtpSection["FromEmail"] ?? _smtpUsername;
        _fromName = smtpSection["FromName"] ?? "EVehicle System";
    }

    /// <summary>
    /// Gửi email OTP để verify email
    /// </summary>
    public async Task SendOtpEmailAsync(string email, string otpCode, string fullName = "")
    {
        try
        {
            var subject = "Xác nhận email đăng ký tài khoản EVehicle";
            var body = GenerateOtpEmailBody(otpCode, fullName);

            await SendEmailAsync(email, subject, body);
            
            _logger.LogInformation("Đã gửi OTP email thành công đến: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi OTP email đến: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Gửi email chào mừng sau khi verify thành công
    /// </summary>
    public async Task SendWelcomeEmailAsync(string email, string fullName = "")
    {
        try
        {
            var subject = "Chào mừng đến với EVehicle!";
            var body = GenerateWelcomeEmailBody(fullName);

            await SendEmailAsync(email, subject, body);
            
            _logger.LogInformation("Đã gửi welcome email thành công đến: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi welcome email đến: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Gửi email sử dụng SMTP
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            EnableSsl = _enableSsl,
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail));

        await client.SendMailAsync(message);
    }

    /// <summary>
    /// Tạo nội dung email OTP
    /// </summary>
    private string GenerateOtpEmailBody(string otpCode, string fullName)
    {
        var name = string.IsNullOrEmpty(fullName) ? "Bạn" : fullName;
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #4CAF50; text-align: center; padding: 20px; background-color: white; margin: 20px 0; letter-spacing: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Xác nhận email đăng ký</h1>
        </div>
        <div class='content'>
            <p>Xin chào {name},</p>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại EVehicle. Để hoàn tất đăng ký, vui lòng sử dụng mã OTP sau để xác nhận email của bạn:</p>
            <div class='otp-code'>{otpCode}</div>
            <p>Mã OTP này có hiệu lực trong 10 phút. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
            <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
        </div>
        <div class='footer'>
            <p>Trân trọng,<br>Đội ngũ EVehicle</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Tạo nội dung email chào mừng
    /// </summary>
    private string GenerateWelcomeEmailBody(string fullName)
    {
        var name = string.IsNullOrEmpty(fullName) ? "Bạn" : fullName;
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Chào mừng đến với EVehicle!</h1>
        </div>
        <div class='content'>
            <p>Xin chào {name},</p>
            <p>Email của bạn đã được xác nhận thành công. Tài khoản của bạn đã sẵn sàng sử dụng.</p>
            <p>Bây giờ bạn có thể:</p>
            <ul>
                <li>Đăng tin bán xe điện và pin</li>
                <li>Tìm kiếm và mua sản phẩm</li>
                <li>Tham gia đấu giá</li>
                <li>Và nhiều tính năng khác...</li>
            </ul>
            <p>Chúc bạn có trải nghiệm tuyệt vời với EVehicle!</p>
        </div>
        <div class='footer'>
            <p>Trân trọng,<br>Đội ngũ EVehicle</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
    }
}

