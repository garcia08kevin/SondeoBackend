using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.UserControl;
using SondeoBackend.Models;
using System.Web.WebPages;

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
        [Route("ChangeImagen")]
        public async Task<IActionResult> ChangeImagen([FromForm] IFormFile imagen, int userId)
        {
            var user_exist = await _userManager.FindByIdAsync($"{userId}");
            if(user_exist == null)
            {
                return Ok(new UserResult() { Result = false, Respose = "No se encontro el usuario" });
            }
            byte[] bytes = null;
            if (imagen != null)
            {
                using (BinaryReader br = new BinaryReader(imagen.OpenReadStream()))
                {
                    bytes = br.ReadBytes((int)imagen.Length);
                }
                user_exist.Imagen = bytes == null ? null : bytes;
                await _context.SaveChangesAsync();
                return Ok(new UserResult()
                {
                    Result = true,
                    Respose = "Imagen cambiada exitosamente"
                });
            }
            return Ok(new UserResult() { Result = false, Respose = "No se encontro imagen por agregar o remplazar" });
        }

        [HttpPost]
        [Route("ChangeName")]
        public async Task<IActionResult> ChangeName(int userId, string nombre, string apellido)
        {
            var user_exist = await _userManager.FindByIdAsync($"{userId}");
            if (user_exist == null)
            {
                return Ok(new UserResult() { Result = false, Respose = "No se encontro el usuario" });
            }
            user_exist.Name = nombre.IsEmpty() ? user_exist.Name : nombre ;
            user_exist.Lastname = apellido.IsEmpty() ? user_exist.Lastname : apellido;
            await _context.SaveChangesAsync();
            return Ok(new UserResult()
            {
                Result = true,
                Respose = "Datos del usuario cambiados exitosamente"
            });
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserVerification verification)
        {
            try
            {
                var user_exist = await _userManager.FindByNameAsync(verification.UserName);
                if (user_exist == null)
                {
                    return Ok(new UserResult() { Result = false, Respose = "El usuario no esta registrado" });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(user_exist, verification.OldPassword);
                if (!isCorrect)
                {
                    return Ok(new UserResult() { Result = false, Respose = "Clave de usuario es incorrecta" });
                }
                if (!verification.Password.Equals(verification.ConfirmPassword))
                {
                    return Ok(new UserResult() { Result = false, Respose = "Las contraseñas no coinciden" });
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
                            Mensaje = $"El usuario {user_exist.UserName} ha activado su cuenta",
                            Identificacion = user_exist.Id
                        };
                        _context.Notifications.Add(notificacion);
                        await _context.SaveChangesAsync();
                        await _hubs.Clients.All.SendAsync("Notificacion", notificacion.Mensaje);
                    }
                    return Ok(new UserResult()
                    {
                        Result = true,
                        Respose = "La contraseña ha sido cambiada exitosamente"
                    });
                }
                return BadRequest(error: new UserResult()
                {
                    Result = false,
                    Respose = "No se pudo cambiar la contraseña"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(error: new UserResult()
                {
                    Result = false,
                    Respose = $"No se pudo cambiar la contraseña {ex.Message}"
                });
            }         
        }
    }
}
