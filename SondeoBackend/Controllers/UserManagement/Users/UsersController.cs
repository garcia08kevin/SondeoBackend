using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.UserControl;
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
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserVerification verification)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(verification.Email);
                if (user_exist == null)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "El usuario no esta registrado"
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(user_exist, verification.OldPassword);
                if (!isCorrect)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "Clave de usuario es incorrecta"
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
                    return Ok(new UserResult()
                    {
                        Result = true,
                        Respose = "La contraseña ha sido cambiada exitosamente"
                    });
                }
            }
            return BadRequest(error: new UserResult()
            {
                Result = false,
                Respose = "No se pudo cambiar la contraseña"
            });
        }
    }
}
