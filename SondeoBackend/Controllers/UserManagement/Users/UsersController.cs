using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.Models;
using System.IdentityModel.Tokens.Jwt;

namespace SondeoBackend.Controllers.UserManagement.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ILogger<UsersController> _logger;
        private readonly IHubContext<Hubs> _hubs;

        public UsersController(UserManager<CustomUser> userManager, IHubContext<Hubs> hubs, ILogger<UsersController> logger, DataContext context)
        {
            _hubs = hubs;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("CurrentUser")]
        public async Task<IActionResult> CurrentUser([FromBody] ModelResult user)
        {
            if (ModelState.IsValid)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(user.Token);
                var email = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "email");
                if (email == null)
                {
                    return BadRequest(error: new ModelResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "No se ha encontrado el email del usuario"
                        }
                    });
                }
                var user_exist = await _userManager.FindByEmailAsync(email.Value);
                if (user_exist == null)
                {
                    return BadRequest(error: new ModelResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "El usuario no esta registrado"
                        }
                    });
                }
                var role = await _userManager.GetRolesAsync(user_exist);
                return Ok(new UserDetail()
                {
                    Id = user_exist.Id,
                    Name = user_exist.Name,
                    Lastname = user_exist.Lastname,
                    Role = role[0],
                    Email = user_exist.Email,
                    Activado = user_exist.CuentaActiva,
                    CorreoActivado = user_exist.EmailConfirmed
                });
            }
            return BadRequest(error: new ModelResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se pudo obtener los datos del usuario"
                        }
            });
        }

        [HttpPost]
        [Route("UserDetailById")]
        public async Task<IActionResult> UserDetailById(int id)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByIdAsync(Convert.ToString(id));
                if (user_exist == null)
                {
                    return BadRequest(error: new ModelResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "El usuario no esta registrado"
                        }
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
            return BadRequest(error: new ModelResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se pudo obtener los datos del usuario"
                        }
            });
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserVerification verification)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(verification.Email);
                if (user_exist == null)
                {
                    return BadRequest(error: new ModelResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "El usuario no esta registrado"
                        }
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(user_exist, verification.OldPassword);
                if (!isCorrect)
                {
                    return BadRequest(error: new ModelResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Clave de usuario es incorrecta"
                        }
                    });
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user_exist);
                var result = await _userManager.ResetPasswordAsync(user_exist, token, verification.Password);
                if (result.Succeeded)
                {
                    if (!user_exist.EmailConfirmed)
                    {
                        user_exist.EmailConfirmed = true;
                        var notificacion = new Notification()
                        {
                            Tipo = 1,
                            Fecha = DateTime.Now,
                            Mensaje = $"El usuario {user_exist.Email} ha activado su cuenta",
                            Identificacion = user_exist.Id
                        };
                        _context.Notifications.Add(notificacion);
                        await _context.SaveChangesAsync();
                        await _hubs.Clients.All.SendAsync("Notificacion", notificacion.Mensaje);

                    }
                    return Ok(new ModelResult()
                    {
                        Result = true,
                        Contenido = "La contraseña ha sido cambiada exitosamente"
                    });
                }
            }
            return BadRequest(error: new ModelResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se pudo cambiar la contraseña"
                        }
            });
        }
    }
}
