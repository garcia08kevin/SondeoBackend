using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO.Result;
using SondeoBackend.DTO.UserControl;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SondeoBackend.Controllers.UserManagement.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<CustomRole> _roleManager;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(UserManager<CustomUser> userManager, IConfiguration configuration, RoleManager<CustomRole> roleManager, ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLogin login)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(login.Email);
                if (user_exist == null)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "Usuario No registrado"
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(user_exist, login.Password);
                if (!isCorrect)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "Clave de Usuario Incorrecta"
                    });
                }
                if (!user_exist.EmailConfirmed)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "FirstLogin"
                    });
                }
                if (!user_exist.CuentaActiva)
                {
                    return BadRequest(error: new UserResult()
                    {
                        Result = false,
                        Respose = "Tu usuario ha sido bloqueado"
                    });
                }
                var jwtToken = await GenerateToken(user_exist);
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
                var role = await _userManager.GetRolesAsync(user_exist);
                var email = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "email");
                var userData = new UserDetail()
                {
                    Id = user_exist.Id,
                    Name = user_exist.Name,
                    Lastname = user_exist.Lastname,
                    Role = role[0],
                    Email = user_exist.Email,
                    Activado = user_exist.CuentaActiva,
                    CorreoActivado = user_exist.EmailConfirmed,
                    Alias = user_exist.Alias
                };
                return Ok(new UserResult()
                {
                    Result = true,
                    Token = jwtToken,
                    User = userData
                });
            }
            return BadRequest(error: new UserResult()
            {
                Result = false,
                Respose = "Error ingreso del usuario"
            });
        }

        [HttpGet]
        [Route("GetValidClaims")]
        public async Task<List<Claim>> GetValidClaims(CustomUser user)
        {
            var options = new IdentityOptions();
            var claims = new List<Claim>
            {
                new Claim(type: "Id", value: Convert.ToString(user.Id)),
                    new Claim(type: JwtRegisteredClaimNames.Sub, value: user.Email),
                    new Claim(type: JwtRegisteredClaimNames.Email, value: user.Email),
                    new Claim(type: JwtRegisteredClaimNames.Jti, value: Guid.NewGuid().ToString()),
                    new Claim(type: JwtRegisteredClaimNames.Iat, value: DateTime.UtcNow.ToUniversalTime().ToString())
            };
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            return claims;
        }
        private async Task<string> GenerateToken(CustomUser? user)
        {
            var jwt_token = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection(key: "JwtConfig:Secret").Value);
            var claims = await GetValidClaims(user);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = jwt_token.CreateToken(tokenDescriptor);
            return jwt_token.WriteToken(token);
        }
    }
}
