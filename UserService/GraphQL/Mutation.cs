using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;

namespace UserService.GraphQL
{
    public class Mutation
    {
        //===========================REGISTER USER AS BUYER FOR DEFAULT===================================//
        public async Task<UserData> RegisterUserAsync(
        RegisterUser input,
        [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.UserName == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                UserName = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            var memberRole = context.Roles.Where(m => m.Name == "BUYER").FirstOrDefault();
            if (memberRole == null)
                throw new Exception("Invalid Role");
            var userRole = new UserRole
            {
                RoleId = memberRole.Id,
                UserId = newUser.Id
            };
            newUser.UserRoles.Add(userRole);
            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();



            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.UserName,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }

        //========================================LOGIN USER==============================================//
        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings, // setting token
            [Service] FoodDeliveringContext context) // EF
        {
            var user = context.Users.Where(o => o.UserName == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));

                var userRoles = context.UserRoles.Where(o => o.Id == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }

        //========================================UPDETE USER BY ADMIN==========================================//
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> UpdateUserAsync(
            UserInput input,
            [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.FullName = input.FullName;
                user.UserName = input.UserName;
                user.Email = input.Email;
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }

        //========================================DELETE USER BY ADMIN==========================================//
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> DeleteUserByIdAsync(
            int id,
            [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

        //========================================ADD PROFILE BY USER==========================================//
        [Authorize]
        public async Task<Profile> AddProfileAsync(
            ProfilesInput input,
            [Service] FoodDeliveringContext context)
        {
            var profile = new Profile
            {
                UserId = input.UserId,
                Name = input.Name,
                Address = input.Address,
                Phone = input.Phone

            };
            var ret = context.Profiles.Add(profile);
            await context.SaveChangesAsync();
            return ret.Entity;

        }

        //========================================UPDATE USER ROLE BY ADMIN=====================================//
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<UserRole> UpdateUserRoleAsync(
            UserRoleInput input,
            [Service] FoodDeliveringContext context)
        {
            var userRole = context.UserRoles.Where(o => o.Id == input.Id).FirstOrDefault();
            if (userRole != null)
            {
                userRole.UserId = input.UserId;
                userRole.RoleId = input.RoleId;

                context.UserRoles.Update(userRole);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(userRole);
        }

        //===================================CHANGE PASSWORD BY USER===========================================//
        [Authorize]
        public async Task<UserData> ChangePasswordAsync(
            ChangePassword input,
            [Service] FoodDeliveringContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userToken = claimsPrincipal.Identity;
            var user = context.Users.Where(u => u.UserName == userToken.Name).FirstOrDefault();
            if (user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(new UserData
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName,
                Email = user.Email,
            });
        }


        //=======================================MANAGE COURIER=========================================//
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<UserData> AddCourierAsync(
        RegisterUser input,
        [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.UserName == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                UserName = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            var memberRole = context.Roles.Where(m => m.Name == "COURIER").FirstOrDefault();
            if (memberRole == null)
                throw new Exception("Invalid Role");
            var userRole = new UserRole
            {
                RoleId = memberRole.Id,
                UserId = newUser.Id
            };
            newUser.UserRoles.Add(userRole);
            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.UserName,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }

        //==========================================UPDATE COURIER BY MANAGER=============================//
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<UserRole> UpdateCourierAsync(
            UserInput input,
            [Service] FoodDeliveringContext context)
        {
            
            var userRole = context.UserRoles.Where(o => o.Id == input.Id).FirstOrDefault();
            var user = context.Users.Where(o => o.Id == input.Id && userRole.RoleId == 4).FirstOrDefault();
            if (userRole != null)
            {
                user.FullName = input.FullName;
                user.UserName = input.UserName;
                user.Email = input.Email;
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(userRole);
        }        

    }
 }
