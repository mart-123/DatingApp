using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDTO userForRegisterDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // validate request (this now happens by virtue of 'ApiController' directive above)
            // userForRegisterDTO.Username = userForRegisterDTO.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDTO.Username))
                return BadRequest("username already exists");

            /* TERRIBLE PRACTICE?! OR 'LAZY' (DEFERRED CREATION - OPTIMISATION STRATEGY) INSTANTIATING A */
            /* SKELETON OBJECT TO HOLD A DISEMBODIED PROPERTY WITH OTHER PROPERTIES STILL BEING NULL!!! */
            /* IT IS A MEANINGLESS OBJECT, HAVING HARDLY ANY OF THE PROPERTIES ESSENTIAL TO A USER!! */
            var userToCreate = new User
            {
                UserName = userForRegisterDTO.Username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDTO.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTO userForLoginDTO)
        {
            var userFromRepo = await _repo.Login(userForLoginDTO.Username.ToLower(), userForLoginDTO.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // generate a token...
            // starting with some basic identifying details of the user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            // generate a hashed security key to sign the token
            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_config.GetSection("AppSettings:Token").Value));
            
            // generate signing credentials (by hashing the key)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {             // anonymous type
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}