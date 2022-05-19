using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using UserService.Models;

namespace UserService.GraphQL
{
    public class Query
    {     

        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] FoodDeliveringContext context) =>
            context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.UserName
            });

        [Authorize]
        public IQueryable<Profile> GetProfilesbyToken([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
            if (user != null)
            {
                var profiles = context.Profiles.Where(o => o.UserId == user.Id);
                return profiles.AsQueryable();
            }
            return new List<Profile>().AsQueryable();
        }

        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses dengan role admin
        public IQueryable<UserRole> GetUserRoles([Service] FoodDeliveringContext context) =>
            context.UserRoles.Select(p => new UserRole()
            {
                Id = p.Id,
                UserId = p.UserId,
                RoleId = p.RoleId
            });

    }
}
