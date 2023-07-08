using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.UserControl;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers.UserManagement.Administrador
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<CustomRole> _roleManager;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(DataContext context, SignInManager<CustomUser> signInManager, UserManager<CustomUser> userManager, IConfiguration configuration, RoleManager<CustomRole> roleManager, ILogger<AccountsController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _logger = logger;
        }

        #region Manage Users
        [HttpPost]
        [Route("Registro")]
        public async Task<IActionResult> Register([FromBody] UserRegistration user)
        {
            if (ModelState.IsValid)
            {
                var alias = "";
                var user_exist = await _userManager.FindByEmailAsync(user.Email);
                if (user_exist != null)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "El correo ingresado ya existe"
                    });
                }
                var role_exist = await _roleManager.FindByNameAsync(user.Role);
                if (role_exist == null)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "El Rol no esta registrado en el sistema"
                    });
                }
                else
                {
                    if (user.Role.Equals("Administrador"))
                    {
                        alias = $"{user.Lastname[0]}{user.Name}_Admin";
                    }
                    else
                    {
                        alias = $"{user.Lastname[0]}{user.Name}_Encue";
                    }
                    var new_user = new CustomUser()
                    {
                        Email = user.Email,
                        UserName = user.Email,
                        Name = user.Name,
                        Lastname = user.Lastname,
                        Alias = alias.ToUpper()
                    };
                    var pass_generate = GenerateRandomPassword();
                    var is_create = await _userManager.CreateAsync(new_user, pass_generate);
                    if (is_create.Succeeded)
                    {
                        var body = $"<div class=\"homePage\">\r\n    <div class=\"col-xs-12 col-sm-12 col-md-offset-2 col-md-10 col-lg-offset-2 col-lg-10\">\r\n        <div class=\"container col-md-12 col-lg-12\">\r\n            <!--Inicio recuperar contraseña -->\r\n     <h2 class=\"form-signin-heading recuperarPassTitl text-center\">Tu cuenta ha sido creada</h2>\r\n     <p class=\"subtreestablecerPass text-center\">Hola nombre, tu cuenta en Sondeo ha sido activada, estas son tus credenciales:</p>       \r\n        <ul>\r\n            <li> Email: {new_user.Email} XD</li>\r\n            <li> Contraseña: {pass_generate} </li>\r\n            <li> Rol: {user.Role} ss </li>        \r\n        </ul>\r\n    </div> \r\n </div> ";
                        SendEmail(body, new_user.Email, "CuentaActivada");
                        await _userManager.AddToRoleAsync(new_user, user.Role);
                        return Ok(new UserResult()
                        {
                            Result = true,
                            Token = pass_generate,
                            Respose = "El usuario a sido generado con exito esta es su clave temporal"
                        });
                    }
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "Error al registrar el usuario"
                    });
                }
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("GetUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("GetUsersById")]
        public async Task<IActionResult> GetUsersById(int id)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByIdAsync(Convert.ToString(id));
                if (user_exist == null)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "El usuario no esta registrado"
                    });
                }
                var role = await _userManager.GetRolesAsync(user_exist);
                return Ok(new UserDetail()
                {
                    Name = user_exist.Name,
                    Lastname = user_exist.Lastname,
                    Role = role[0],
                    Email = user_exist.Email,
                    Activado = user_exist.CuentaActiva,
                    CorreoActivado = user_exist.EmailConfirmed
                });
            }
            return BadRequest(error: new UserResult()
            {
                Result = false,
                Respose = "No se pudo obtener los datos del usuario"
            });
        }

        [HttpGet]
        [Route("GetAllUserByRole")]
        public async Task<IActionResult> GetAllUserByRole(string role)
        {
            List<CustomUser> users = new List<CustomUser>();
            var usuariosActivados = await _userManager.Users.Where(e => e.CuentaActiva == true).ToListAsync();
            for (int i = 0; i < usuariosActivados.Count; i++)
            {
                var userRole = await _userManager.GetRolesAsync(usuariosActivados[i]);
                if (userRole[0].Equals(role))
                {
                    users.Add(usuariosActivados[i]);
                }
            }
            return Ok(users);
        }

        [HttpPost]
        [Route("ActivarUsuario")]
        public async Task<bool> ActivarUsuario(string email, bool eleccion)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(email);
                if (user_exist == null)
                {
                    return false;
                }
                var user = _context.Users.First(a => a.Email == email);
                if (!eleccion)
                {
                    user.CuentaActiva = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                user.CuentaActiva = true;
                await _context.SaveChangesAsync();
                return true;

            }
            return false;
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(int userId)
        {
            var user_exist = await _userManager.FindByIdAsync(Convert.ToString(userId));
            if (user_exist == null)
            {
                return BadRequest(error: new UserResult()
                {
                    Result = false,
                    Respose = "El usuario no esta registrado"
                });
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user_exist);
            var password = GenerateRandomPassword();
            var result = await _userManager.ResetPasswordAsync(user_exist, token, password);
            if (result.Succeeded)
            {
                user_exist.EmailConfirmed = false;
                await _context.SaveChangesAsync();
                return Ok(new UserResult()
                {
                    Result = true,
                    Token = password,
                    Respose = "Se ha reseteado el usuario con exito"
                });
            }
            return BadRequest(error: new UserResult()
            {
                Result = false,
                Respose = "No se pudo resetar la contraseña del usuario"
            });
        }

        [HttpDelete]
        [Route("RemoveUser")]
        public async Task<IActionResult> RemoveUser(int id)
        {
            var user = await _userManager.FindByIdAsync(Convert.ToString(id));
            if (user == null)
            {
                _logger.LogInformation($"El usuario no exite");
                return BadRequest(new UserResult
                {
                    Result = false,
                    Respose = $"El usuario no exite"
                });
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new UserResult
                {
                    Result = true,
                    Respose = "Se ha Eliminado el Usuario"
                });
            }
            else
            {
                _logger.LogInformation("No se pudo eliminar el usuario");
                return BadRequest(new UserResult { Result = false, Respose = "No se pudo eliminar el usuario" });
            }
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
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",
            "abcdefghijkmnopqrstuvwxyz",
            "0123456789",
            "!@$?_-"
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
        #endregion

        #region Manage Roles

        [HttpPost]
        [Route("AddUserRole")]
        public async Task<IActionResult> AddUserRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"El usuario {email} no exite");
                return BadRequest(new { error = $"El usuario {email} no exite" });
            }
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                _logger.LogInformation($"El rol {roleName} no exite");
                return BadRequest(new { error = $"El rol {roleName} no exite" });
            }
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = "Role agregado correctamente"
                });
            }
            else
            {
                _logger.LogInformation("No se pudo agregar el rol");
                return BadRequest(new { error = "No se pudo agregar el rol" });
            }
        }

        [HttpPost]
        [Route("RemoveUserRole")]
        public async Task<IActionResult> RemoveUserRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"El usuario {email} no exite");
                return BadRequest(new { error = $"El usuario {email} no exite" });
            }
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                _logger.LogInformation($"El rol {roleName} no exite");
                return BadRequest(new { error = $"El rol {roleName} no exite" });
            }
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = "Se ha retirado el rol correctamente"
                });
            }
            else
            {
                _logger.LogInformation("No se pudo retirar el rol");
                return BadRequest(new { error = "No se pudo retirar el rol" });
            }
        }
        #endregion

        #region Notificaciones
        [Route("Notificaciones")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            if (_context.Notifications == null)
            {
                return NotFound();
            }
            return await _context.Notifications.OrderByDescending(p => p.Fecha).ToListAsync();
        }

        [Route("Notificaciones/MarcarComoLeido")]
        [HttpPost]
        public async Task<IActionResult> MarcarComoLeido(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (!notification.Vista)
            {
                notification.Vista = true;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            notification.Vista = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Route("Notificaciones/NoLeidas")]
        [HttpGet]
        public async Task<int> NotificacionesNoLeidas()
        {
            int cantidad = 0;
            var notificaciones = await _context.Notifications.ToListAsync();
            for (int i = 0; i < notificaciones.Count; i++)
            {
                if (!notificaciones[i].Vista)
                {
                    cantidad++;
                }
            }
            return cantidad;
        }

        [HttpDelete("Notificaciones/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            if (_context.Notifications == null)
            {
                return NotFound();
            }
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
