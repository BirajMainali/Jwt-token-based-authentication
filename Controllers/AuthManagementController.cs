using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplication.Configuration;
using WebApplication.Dto.Request;
using WebApplication.Dto.Response;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")] // api authManagement
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AuthManagementController(UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if (!ModelState.IsValid) return AuthResponse("Invalid Payload");
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null) return AuthResponse("Email already in use");
            var newUser = new IdentityUser { Email = user.Email, UserName = user.UserName };
            var isCreated = await _userManager.CreateAsync(newUser, user.Password);
            if (!isCreated.Succeeded)
            {
                return BadRequest(new RegistrationResponse()
                    { Errors = isCreated.Errors.Select(x => x.Description).ToList(), Success = false });
            }

            var jwtToken = GenerateJwtToken(newUser);
            return Ok(new RegistrationResponse()
            {
                Success = true,
                Token = jwtToken
            });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (!ModelState.IsValid) return AuthResponse("Invalid Payload");
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser == null) return AuthResponse("Invalid Login Request");
            var isValid = await _userManager.CheckPasswordAsync(existingUser, user.Password);
            if (!isValid) return AuthResponse("Invalid Email or Password");
            var jwtToken = GenerateJwtToken(existingUser);
            return Ok(new RegistrationResponse()
            {
                Success = true,
                Token = jwtToken
            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var jwtDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jwtHandler.CreateToken(jwtDescriptor);
            var jwtToken = jwtHandler.WriteToken(token);
            return jwtToken;
        }

        private BadRequestObjectResult AuthResponse(string message)
        {
            return BadRequest(new RegistrationResponse()
                { Errors = new List<string>() { message }, Success = false });
        }
    }
}