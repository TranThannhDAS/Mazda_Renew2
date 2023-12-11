using Mazda.Base;
using Mazda.Data.UnitofWork;
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
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mazda.Controllers
{
    public class UserController : BaseController
    {
        public UserController(IUnitofWork unitofWork, DataContext dataContext, IConfiguration configuration) : base(unitofWork, dataContext, configuration)
        {
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
              if(pass)
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
                    if(check > 0)
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
            return BadRequest();
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
                    return BadRequest("Invalid client request");
                }

                string? accessToken = tokenModel.AccessToken;
                string? refreshToken = tokenModel.RefreshToken;

                var principal = GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    return BadRequest("Invalid access token or refresh token");
                }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                string username = principal.Identity.Name;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                var user = await DataContext.Users.Where(query => query.UserName.Equals(username)).FirstOrDefaultAsync();


                if (user == null || user.RefreshToken != refreshToken || user.ExpireTime <= DateTime.Now)
                {
                    return BadRequest("Invalid access token or refresh token, cho người dùng login lại");
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
            }catch(Exception ex) 
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
            var user = await DataContext.Users.Skip((panigation.PageIndex -1)* panigation.PageSize)
                       .Take(panigation.PageSize).ToListAsync();
            return Ok(user);
        }
    }
}
