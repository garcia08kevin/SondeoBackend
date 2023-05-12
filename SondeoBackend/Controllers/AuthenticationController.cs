﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SondeoBackend.Configuration;
using SondeoBackend.Context;
using SondeoBackend.DTO;
using SondeoBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
 
namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<CustomRole> _roleManager;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly AdminController _adminController;

        public AuthenticationController(UserManager<CustomUser> userManager, IConfiguration configuration, SignInManager<CustomUser> signInManager, RoleManager<CustomRole> roleManager, ILogger<AuthenticationController> logger, DataContext context, AdminController adminController)
        {
            _adminController = adminController;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
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
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Usuario No registrado"
                        }
                    });
                }
                if (!user_exist.CuentaActiva)
                {
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Usuario no activado, debe cambiar su contraseña"
                        }
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(user_exist, login.Password);
                if (!isCorrect)
                {
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Clave de Usuario Incorrecta"
                        }
                    });
                }
                var jwtToken = GenerateToken(user_exist);
                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = await jwtToken
                });
            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "Error ingreso del usuario"
                        }
            });
        }

        [HttpPost]
        [Route("IsAdmin")]
        public bool IsAdmin([FromBody] AuthResult user)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(user.Token);
            var role = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "role");
            if (role.Value.Equals("Administrador"))
            {
                return true;
            }
            return false;

        }

        [HttpPost]
        [Route("UserDetail")]
        public async Task<IActionResult> UserDetail(string email)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(email);
                if (user_exist == null)
                {
                    return BadRequest(error: new AuthResult()
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
                    Role = role[0]
                });
            }
            return BadRequest(error: new AuthResult()
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
                    return BadRequest(error: new AuthResult()
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
                    Activado = user_exist.CuentaActiva
                });
            }
            return BadRequest(error: new AuthResult()
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
                    return BadRequest(error: new AuthResult()
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
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Clave de usuario es incorrecta"
                        }
                    });
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user_exist);
                var result = await _userManager.ResetPasswordAsync(user_exist, token,verification.Password);
                if (result.Succeeded)
                {
                    if (!user_exist.CuentaActiva)
                    {
                        await _adminController.ActivarUsuario(user_exist.Email,true);
                    }
                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Contenido = "La contraseña ha sido cambiada exitosamente"
                    });
                }                
            }
            return BadRequest(error: new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                        {
                            "No se pudo cambiar la contraseña"
                        }
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
        private async Task<String> GenerateToken(CustomUser? user)
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
