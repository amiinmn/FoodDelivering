using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using UserService.Models;

namespace UserService.GraphQL
{
    public class Query
    {
        //-------------------------------------VIEW ALL USER BY ADMIN--------------------------------------------//
        [Authorize(Roles = new[] { "ADMIN" })] 
        public IQueryable<UserData> GetUsers(
            [Service] FoodDeliveringContext context) =>
            context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.UserName
            });

        //-------------------------------------VIEW ALL USER ROLE BY ADMIN---------------------------------------//
        [Authorize(Roles = new[] { "ADMIN" })] 
        public IQueryable<UserRole> GetUserRoles(
            [Service] FoodDeliveringContext context) =>
            context.UserRoles.Select(p => new UserRole()
            {
                Id = p.Id,
                UserId = p.UserId,
                RoleId = p.RoleId
            });

        //-------------------------------------VIEW PROFILE BY TOKEN--------------------------------------------//
        [Authorize]
        public IQueryable<Profile> GetProfilesbyToken(
            [Service] FoodDeliveringContext context, 
            ClaimsPrincipal claimsPrincipal)
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


        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<User> GetCouriers(
            [Service] FoodDeliveringContext context)
        {
            var userRole = context.Roles.Where(k => k.Name == "COURIER").FirstOrDefault();
            var courier = context.Users.Where(k => k.UserRoles.Any(o => o.RoleId == userRole.Id));
            return courier.AsQueryable();
        }

    }
}
