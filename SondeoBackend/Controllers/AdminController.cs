using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthenticationController> _logger;

        public AdminController(DataContext context, SignInManager<CustomUser> signInManager, UserManager<CustomUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, ILogger<AuthenticationController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("Registro")]
        public async Task<IActionResult> Register([FromBody] UserRegistration user)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(user.Email);
                if (user_exist != null)
                {
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "El correo ingresado ya existe"
                        }
                    });
                }
                var new_user = new CustomUser()
                {
                    Email = user.Email,
                    UserName = user.Email,
                    Name = user.Name,
                    Lastname = user.Lastname
                };
                var role_exist = await _roleManager.FindByNameAsync(user.Role);
                if (role_exist == null)
                {
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "El Rol no esta registrado en el sistema"
                        }
                    });
                }
                else
                {
                    var pass_generate = GenerateRandomPassword();
                    var is_create = await _userManager.CreateAsync(new_user, pass_generate);
                    if (is_create.Succeeded)
                    {
                        var body = $"<div class=\"homePage\">\r\n    <div class=\"col-xs-12 col-sm-12 col-md-offset-2 col-md-10 col-lg-offset-2 col-lg-10\">\r\n        <div class=\"container col-md-12 col-lg-12\">\r\n            <!--Inicio recuperar contraseña -->\r\n     <h2 class=\"form-signin-heading recuperarPassTitl text-center\">Tu cuenta ha sido creada</h2>\r\n     <p class=\"subtreestablecerPass text-center\">Hola nombre, tu cuenta en Sondeo ha sido activada, estas son tus credenciales:</p>       \r\n        <ul>\r\n            <li> Email: {new_user.Email} XD</li>\r\n            <li> Contraseña: {pass_generate} </li>\r\n            <li> Rol: {user.Role} ss </li>        \r\n        </ul>\r\n    </div> \r\n </div> ";
                        SendEmail(body, new_user.Email, "CuentaActivada");
                        await _userManager.AddToRoleAsync(new_user, user.Role);
                        return Ok(new AuthResult()
                        {
                            Contenido = $"El usuario a sido generado con exito esta es su clave temporal {pass_generate}"
                        });
                    }
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Error al registrar el usuario"
                        }
                    });
                }
            }
            return BadRequest();
        }

        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
        public static void SendEmail(string body, string destinatario, string subject)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("bradley.emard@ethereal.email"));
            email.To.Add(MailboxAddress.Parse(destinatario));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.ethereal.email", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("bradley.emard@ethereal.email", "HRja57PgmHAFRamyPw");
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
