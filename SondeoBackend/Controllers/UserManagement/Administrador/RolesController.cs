using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SondeoBackend.Configuration;
using SondeoBackend.Context;

namespace SondeoBackend.Controllers.UserManagement.Administrador
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<CustomRole> _roleManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(UserManager<CustomUser> userManager, RoleManager<CustomRole> roleManager, ILogger<RolesController> logger)
        {
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
            return Ok(new
            {
                result = roles
            });
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
    }
}
