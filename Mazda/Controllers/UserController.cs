using AutoMapper;
using Mazda.Base;
using Mazda.Data.Services;
using Mazda.Data.UnitofWork;
using Mazda.Dtos.User;
using Mazda.Model;
using Mazda_Api.Controllers;
using Mazda_Api.Dtos;
using Mazda_Api.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mazda.Controllers
{
    public class UserController : BaseController
    {
        private readonly SmtpClient _smtpClient;
        private IMapper mapper;
        private readonly IUserService userService;
        public UserController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration, IUserService userService) : base(unitofWork, dataContext, configuration)
        {
            this.userService = userService;

            _smtpClient = new SmtpClient
            {
                Host = Configuration["SmtpConfig:SmtpServer"],
                Port = int.Parse(Configuration["SmtpConfig:Port"]),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Configuration["SmtpConfig:Username"],
                    Configuration["SmtpConfig:Password"]
                ),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            this.mapper = mapper;
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserDto userDto)
        {
            if (userDto.UsernameCurrent.Equals("dat1234"))
            {
                var checkUsername = await DataContext.Users.Where(query => query.UserName.Equals(userDto.Username)).FirstOrDefaultAsync();
                if (checkUsername != null)
                {
                    return Ok(new
                    {
                        message = "UserName trùng"
                    });
                }
                User user = new User();
                user.UserName = userDto.Username;
                user.Pass = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
                await UnitofWork.Repository<User>().AddAsync(user);
                int check = await UnitofWork.Complete();
                return Ok(new
                {
                    Id = user.Id
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Bạn không có quyền"
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteById(UserDeleteDto userDeleteDto)
        {
            if (userDeleteDto.UserNameCurrent.Equals("dat1234"))
            {
                var existingUser = await UnitofWork.Repository<User>().GetByIdAsync(userDeleteDto.Id);
                await UnitofWork.Repository<User>().Delete(existingUser);
                var check = await UnitofWork.Complete();
                if (check > 0)
                {
                    return Ok(new
                    {
                        message = "Xóa thành công"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Xóa không thành công"
                    });
                }
            }
            else
            {
                return Ok(new
                {
                    message = "Bạn không có quyền"
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            var checkUsername = await DataContext.Users.Where(query => query.UserName.Equals(userDto.Username)).FirstOrDefaultAsync();
            if (checkUsername != null)
            {
                var pass = BCrypt.Net.BCrypt.Verify(userDto.Password, checkUsername.Pass);
                if (pass)
                {
                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userDto.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                    var token = CreateToken(authClaims);
                    var refreshToken = GenerateRefreshToken();
                    _ = int.TryParse(Configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
                    checkUsername.RefreshToken = refreshToken;
                    checkUsername.ExpireTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                    await UnitofWork.Repository<User>().Update(checkUsername);
                    int check = await UnitofWork.Complete();
                    if (check > 0)
                    {
                        return Ok(new
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            RefreshToken = refreshToken,
                            Expiration = token.ValidTo
                        });
                    }
                }
            }
            return Ok("FALSE");
        }
        [Authorize]
        [HttpGet("{username}")]
        public async Task<IActionResult> Logout(string username)
        {
            var checkUsername = await DataContext.Users.Where(query => query.UserName.Equals(username)).FirstOrDefaultAsync();
            if (checkUsername == null) return BadRequest("Invalid user name");

            checkUsername.RefreshToken = null;
            await UnitofWork.Repository<User>().Update(checkUsername);
            int check = await UnitofWork.Complete();
            if (check > 0)
            {
                return Ok(new
                {
                    message = "Logout thành công"
                });

            }
            return BadRequest();
        }

        //ForgotPass
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SendForgotPasswordEmail([FromBody] ForgotPass request)
        {

            var isEmail = await DataContext.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (isEmail == null)
            {
                return BadRequest("Email not Found");
            }

            var conformatCode = GenerateRandomCode();
            request.expireTime = DateTime.UtcNow.AddMinutes(2);
            request.Code = conformatCode;

            await DataContext.ForgotPasses.AddAsync(request);
            await DataContext.SaveChangesAsync();

            SendConfirmationEmail(isEmail.UserName, conformatCode);

            return Ok(
               new { message = "A confirmation email has been sent to your email address." }
           );
        }

        // Verify code reset password
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCodeResetPassword(
           [FromBody] ForgotPass request
       )
        {
            var confirmCode = await DataContext.ForgotPasses
                .Where(f => f.UserName == request.UserName && f.Code == request.Code)
                .FirstOrDefaultAsync();

            if (confirmCode is null)
            {
                return NotFound(new { message = "confirm code invalid" });
            }

            if (confirmCode.expireTime < DateTime.UtcNow)
            {
                DataContext.ForgotPasses.Remove(confirmCode);
                await DataContext.SaveChangesAsync();
                return NotFound(new { message = "The code has expired" });
            }

            DataContext.ForgotPasses.Remove(confirmCode);
            await DataContext.SaveChangesAsync();
            var reset = new ResetPasswordDto();
            reset.Username = request.UserName;
            var test =  ResetPassword(reset);
            return Ok(new { message = test });
        }


        // Delete confirm code
        [HttpDelete]
        [AllowAnonymous]
        public async Task<ActionResult> DeleteConfirmCodeExpired()
        {
            DateTime currentTime = DateTime.UtcNow;

            var ConfirmCodeExpired = DataContext.ForgotPasses
                .Where(f => f.expireTime < currentTime)
                .ToList();

            if (ConfirmCodeExpired.Count == 0)
            {
                return NotFound(new { message = "Code is not found" });
            }

            foreach (var confirmCode in ConfirmCodeExpired)
            {
                DataContext.ForgotPasses.Remove(confirmCode);
            }

            await DataContext.SaveChangesAsync();

            return Ok(new { message = "Delete successfully" });
        }

        // Send email comfirm code
        private void SendConfirmationEmail(string email, string confirmationCode)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(Configuration["SmtpConfig:Username"]),
                Subject = "Xác nhận quên mật khẩu",
                Body = $"Mã xác nhận của bạn là: {confirmationCode}",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            _smtpClient.Send(mailMessage);
        }
        // Code 
        private string GenerateRandomCode()
        {
            Random random = new Random();
            const string code = "0123456789";
            return new string(
               Enumerable.Repeat(code, 6).Select(s => s[random.Next(s.Length)]).ToArray()
                );
        }

        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePassworkDto user)
        {
            var validUser = await userService.IsValidUserAsync(user.Username, user.OldPassword);
            if (validUser is null)
            {
                return Unauthorized(new { Message = "Incorret username or password !!" });

            }
            validUser.Pass = BCrypt.Net.BCrypt.HashPassword(user.NewPassword);
            DataContext.Entry(validUser).State = EntityState.Modified;
            await DataContext.SaveChangesAsync();




            return Ok(new { Message = "Change password successfully" });
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        public async Task<string> ResetPassword([FromBody] ResetPasswordDto user)
        {
            var validUser =  await DataContext.Users.FirstOrDefaultAsync(u => u.UserName == user.Username);
            if (validUser is null)
            {
                return "Khong tim thay username";
            }
            var newPassword = BCrypt.Net.BCrypt.HashPassword("123456789");
            validUser.Pass = newPassword;
             DataContext.Users.Update(validUser);
             await DataContext.SaveChangesAsync();


            return  "Thanfh cong";
        }
        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]));
            _ = int.TryParse(Configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                issuer: Configuration["JWT:ValidIssuer"],
                audience: Configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        [HttpPost]
        public async Task<IActionResult> RefreshToken(Token tokenModel)
        {
            try
            {
                if (tokenModel is null)
                {
                    return new ObjectResult(new { error = "Invalid client request" }) { StatusCode = 401 }; // Return 500 Internal Server Error
                }

                string? accessToken = tokenModel.AccessToken;
                string? refreshToken = tokenModel.RefreshToken;

                var principal = GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    return new ObjectResult(new { error = "Invalid client request" }) { StatusCode = 401 }; // Return 500 Internal Server Error
                }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                string username = principal.Identity.Name;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                var user = await DataContext.Users.Where(query => query.UserName.Equals(username)).FirstOrDefaultAsync();


                if (user == null || user.RefreshToken != refreshToken || user.ExpireTime <= DateTime.Now)
                {
                    return new ObjectResult(new { error = "Invalid client request" }) { StatusCode = 401 }; // Return 500 Internal Server Error
                }

                var newAccessToken = CreateToken(principal.Claims.ToList());
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                await UnitofWork.Repository<User>().Update(user);
                int check = await UnitofWork.Complete();

                return new ObjectResult(new
                {
                    accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    refreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Có lỗi");
            }
        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }
        [HttpPost]
        public async Task<IActionResult> GetPaginationUser(Panigation panigation)
        {
            var user = await DataContext.Users.Skip((panigation.PageIndex - 1) * panigation.PageSize)
                       .Take(panigation.PageSize).ToListAsync();
            return Ok(user);
        }
    }
}
