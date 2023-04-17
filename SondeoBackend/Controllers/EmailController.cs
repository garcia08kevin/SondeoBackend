using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        [HttpPost]
        public IActionResult SendEmail(string body, string destinatario, string subject)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("bradley.emard@ethereal.email"));
            email.To.Add(MailboxAddress.Parse("bradley.emard@ethereal.email"));
            email.Subject = "Test Email Subject";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) {Text = "<h1 >Tu cuenta ha sido creada</h1>" };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.ethereal.email", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("bradley.emard@ethereal.email", "HRja57PgmHAFRamyPw");
            smtp.Send(email);
            smtp.Disconnect(true);

            return Ok(); 
        }
    }
}
 