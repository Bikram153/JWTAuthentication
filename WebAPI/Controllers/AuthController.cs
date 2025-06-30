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
            response.RefreshToken = token.RefreshToken.Token;

            dataAccess.DisableUserTokenByEmail(request.Email);
            dataAccess.InsertRefreshToken(token.RefreshToken, request.Email);

            return Ok(response);
        }

        [HttpPost("refresh")]
        public ActionResult<AuthResponse> RefreshToken()
        {
            AuthResponse response = new AuthResponse();

            var refreshToken = Request.Cookies["refreshtoken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest();
            }

            var isValid = dataAccess.IsRefreshTokenValid(refreshToken);
            if (!isValid)
            {
                return BadRequest();
            }

            var currentUser = dataAccess.FindUserByToken(refreshToken);
            if (currentUser == null)
            {
                return BadRequest();
            }

            //Generate Access token
            var token = tokenProvider.GenerateToken(currentUser);
            response.AccessToken = token.AccessToken;
            response.RefreshToken = token.RefreshToken.Token;

            dataAccess.DisableUserToken(refreshToken);
            dataAccess.InsertRefreshToken(token.RefreshToken, currentUser.Email);
            return Ok(response);
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshtoken"];
            if (refreshToken != null)
            {
                dataAccess.DisableUserToken(refreshToken);
            }

            return Ok();
        }
    }
}
