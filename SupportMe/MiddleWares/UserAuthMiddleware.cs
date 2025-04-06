using Microsoft.IdentityModel.Logging;
using SupportMe.Data;
using SupportMe.Models;
using SupportMe.Services;
using SupportMe.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace SupportMe.MiddleWares
{
    public class UserAuthMiddleware
    {
        private readonly RequestDelegate _next;
        public UserAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserService _userService, IConfiguration config)
        {
            string token = context.Request.Headers.TryGetValue("Authorization", out var tokenValue) ? tokenValue.ToString() : null;

            User user = null;

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token.Replace("Bearer ", "")) as JwtSecurityToken;
                try
                {
                    user = await _userService.GetUserByToken(token);

                    //if (!user.IsEnabled || user.IsDeleted)
                    //{
                    //    context.Response.StatusCode = 403;
                    //    await context.Response.WriteAsync("USER_NOT_ENABLED");
                    //    return;
                    //}
                }
                catch (UnauthorizedAccessException ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
                catch (Exception ex)
                {
                }
            }


            context.Items["UserMiddelware"] = new UserMiddelware()
            {
                User = user,
                JWT = token,
            };

            if (user != null)
            {
                var identity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("sid", user.Id, ClaimValueTypes.Integer32)
                }, "Custom");

                context.User = new ClaimsPrincipal(identity);
            }

            await _next.Invoke(context);
        }
    }
}
