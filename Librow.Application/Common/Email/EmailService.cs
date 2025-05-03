using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using System.Reflection;


namespace Librow.Application.Common.Email;
public interface IEmailService
{
    Task SendEmailAsync(EmailRequest mailRequest);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(EmailRequest mailRequest)
    {
        var email = new MimeMessage();
        MailboxAddress sender = new MailboxAddress(_emailSettings.DisplayName, _emailSettings.Email);
        email.Sender = sender;
        email.From.Add(sender);
        email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
        email.Subject = mailRequest.Subject;
        var builder = new BodyBuilder();

        // Thêm hình ảnh vào body của email
        byte[] fileBytes;
        if (File.Exists(mailRequest.FileSource))
        {
            FileStream file = new FileStream(mailRequest.FileSource, FileMode.Open, FileAccess.Read);
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }
            builder.Attachments.Add(mailRequest.FileName, fileBytes, ContentType.Parse("application/octet-stream"));
        }

        builder.HtmlBody = mailRequest.Body;
        if (mailRequest.ImageSourceByte != null)
        {
            //attack file vào mail: nếu muốn add ảnh đầu tiên phải attch vào sau đó lấy ra VD attach 2 ảnh, lấy ra ảnh 1 sử dụng cho body thì thay src = cid:{0}
            var image = builder.LinkedResources.Add("img.png", mailRequest.ImageSourceByte);
            image.ContentId = MimeUtils.GenerateMessageId();
            builder.HtmlBody = string.Format(mailRequest.Body, image.ContentId);
        }


        email.Body = builder.ToMessageBody();


        using var smtp = new SmtpClient();
        smtp.Connect(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_emailSettings.Email, _emailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }

}
