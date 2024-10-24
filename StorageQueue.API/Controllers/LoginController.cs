using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StorageQueue.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<string> GetToken([FromQuery] string? person)
        {
            try
            {
                var token = GenerateToken(person ?? "Admin guy");
                return Ok(token);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// This should be in a Service Layer
        /// </summary>
        /// <param name="personName"></param>
        /// <returns></returns>
        private string GenerateToken(string personName)
        {
            var claims = new Claim[]
            {
                new Claim("Name", personName),
                new Claim("Role", "Admin")
            };

            //Read plain decode key from appSettings.json
            var jwtKey = _configuration.GetSection("JWT:Key").Value;
            
            // Convert key to bytes
            var enconded = Encoding.UTF8.GetBytes(jwtKey);

            // Create the object that contain the bytes of the key
            var key = new SymmetricSecurityKey(enconded);
            
            //Encrytp the key using Sha256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Create the token
            var secutiryToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            //Serialize the token to a string
            string token = new JwtSecurityTokenHandler().WriteToken(secutiryToken);

            return token;
        }
    }
}
