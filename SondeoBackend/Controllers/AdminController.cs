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
        private readonly RoleManager<CustomRole> _roleManager;
        private readonly ILogger<AuthenticationController> _logger;

        public AdminController(DataContext context, SignInManager<CustomUser> signInManager, UserManager<CustomUser> userManager, IConfiguration configuration, RoleManager<CustomRole> roleManager, ILogger<AuthenticationController> logger)
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
                            Result = true,
                            Token = pass_generate,
                            Contenido = "El usuario a sido generado con exito esta es su clave temporal"
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
         //La seleccion 1 desactivia el usuario, la 0 lo activa
        [HttpPost]
        [Route("ActivarUsuario")]
        public async Task<bool> ActivarUsuario(string email, int seleccion)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(email);
                if (user_exist == null)
                {
                    return false;
                }
                var user = _context.Users.First(a => a.Email == email);
                if(seleccion == 1)
                {
                    user.CuentaActiva = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                user.CuentaActiva = true;
                var notificacion = new Notification()
                {
                    tipo = 1,
                    fecha = DateTime.Now,
                    Mensaje = $"El usuario {user.Email} ha sido activado"
                };
                _context.Notifications.Add(notificacion);
                await _context.SaveChangesAsync();
                return true;

            }
            return false;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpDelete]
        [Route("RemoveUser")]
        public async Task<IActionResult> RemoveUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation($"El usuario {email} no exite");
                return BadRequest(new { error = $"El usuario {email} no exite" });
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    result = "Se ha Eliminado el Usuario"
                });
            }
            else
            {
                _logger.LogInformation("No se pudo eliminar el usuario");
                return BadRequest(new { error = "No se pudo eliminar el usuario" });
            }
        }


        [Route("ActivarProducto")]
        [HttpPost]
        public async Task<ActionResult> ActivarProducto(int id)
        {
            if (_context.Productos == null)
            {
                return NotFound();
            }
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound("Producto no encontrado");
            }
            if (producto.Activado)
            {
                return Ok("El producto ya ha sido activado");
            }
            producto.Activado = true;
            await _context.SaveChangesAsync();
            return Ok("El producto esta activado");
        }

        [Route("RegistrarProducto")]
        [HttpPost]
        public async Task<ActionResult<Producto>> RegistrarProducto(RegistroProducto registro)
        {
            if (_context.Productos == null)
            {
                return Problem("Entity set 'DataContext.Productos'  is null.");
            }
            var productoAdmin = new Producto()
            {
                Nombre = registro.Nombre,
                CategoriaId = registro.CategoriaId,
                MarcaId = registro.MarcaId,
                PropiedadesId = registro.PropiedadesId,
                Activado = true
            };
            _context.Productos.Add(productoAdmin);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProducto", new { id = productoAdmin.Id }, productoAdmin);
        }
        
        [HttpGet]
        [Route("GetAllRoles")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        [Route("CreateRole")]
        public async Task<IActionResult> CreateRoles(string name)
        {
            var roleExist = await _roleManager.RoleExistsAsync(name);
            if (!roleExist)
            {
                var roleResult = await _roleManager.CreateAsync(new CustomRole(name));
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation($"El rol {name} ha sido a agregado correctamente");
                    return Ok(new
                    {
                        result = $"El rol {name} ha sido a agregado correctamente"
                    });
                }
                else
                {
                    _logger.LogInformation($"El rol {name} no pudo ser creado");
                    return Ok(new
                    {
                        error = $"El rol {name} no pudo ser creado"
                    });
                }
            }
            return BadRequest(new { error = "El Rol no exite" });
        }

        [HttpGet]
        [Route("GetUserRole")]
        public async Task<IActionResult> GetUserRole(int id)
        {
            var user = await _userManager.FindByIdAsync(Convert.ToString(id));
            if (user == null)
            {
                _logger.LogInformation($"El usuario no exite");
                return BadRequest(new { error = $"El usuario  no exite" });
            }
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

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
    }
}
