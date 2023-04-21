using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.CustomIdentity;
using SondeoBackend.Models;
using System.Data;

namespace SondeoBackend.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<CustomRole> _roleManager;
        private readonly ILogger<RolesController> _logger;
        public RolesController(DataContext context, UserManager<CustomUser> userManager, RoleManager<CustomRole> roleManager, ILogger<RolesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
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
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
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
    }
}
