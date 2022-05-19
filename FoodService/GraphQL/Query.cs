using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace FoodService.GraphQL
{
    public class Query
    {
        //[Authorize(Roles = new[] { "MANAGER" })]
        //public IQueryable<Food> GetFoods([Service] FoodDeliveringContext context) =>
        //    context.Foods;

        [Authorize(Roles = new[] { "MANAGER", "BUYER" })]
        public IQueryable<Food> GetFoods([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check manager role ?
            var managerRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
            if (user != null)
            {
                if (managerRole.Value == "MANAGER" || managerRole.Value == "BUYER")
                {
                    return context.Foods;
                }
                var foods = context.Foods;
                return foods.AsQueryable();
            }
            return new List<Food>().AsQueryable();
        }
    }    
}
