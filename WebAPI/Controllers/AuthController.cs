using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;
using WebAPI.Infrastructure;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccess dataAccess;
        private readonly TokenProvider tokenProvider;


        public AuthController(DataAccess dataAccess, TokenProvider tokenProvider)
        {
            this.dataAccess = dataAccess;
            this.tokenProvider = tokenProvider;
        }

        [HttpPost("register")]
        public ActionResult Register(RegisterRequest request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var result = dataAccess.RegisterUser(request.Email, hashedPassword, request.Role);
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public ActionResult<AuthResponse> Login(AuthRequest request)
        {
            AuthResponse response = new AuthResponse();

            var user = dataAccess.FindUserByEmail(request.Email);
            if (user == null)
            {
                return BadRequest("User is not found!");
            }

            var verifyPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!verifyPassword)
            {
                return BadRequest("Wrong Password!");
            }

            //Generate Access token
            var token = tokenProvider.GenerateToken(user);
            response.AccessToken = token.AccessToken;

            //Generate Refresh token



            return response;
        }
    }
}
