using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.AspNetCore.Authorization;

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using OrderService.Models;

namespace OrderService.GraphQL
{
    public class Query
    {
        //[Authorize]
        //public IQueryable<Order> GetOrders(
        //    [Service] FoodDeliveringContext context, 
        //    ClaimsPrincipal claimsPrincipal)
        //{
        //    var userName = claimsPrincipal.Identity.Name;

        //    // check manager role ?
        //    var managerRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role && o.Value == "MANAGER").FirstOrDefault();
        //    var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
        //    if (user != null)
        //    {
        //        if (managerRole != null)
        //            return context.Orders.Include(o => o.OrderDetails);

        //        var orders = context.Orders.Where(o => o.UserId == user.Id);
        //        return orders.AsQueryable();
        //    }

        //    return new List<Order>().AsQueryable();
        //}

        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<Order> GetAllOrder([Service] FoodDeliveringContext context) =>
            context.Orders.Include(o => o.OrderDetails);

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order> GetOrderbyBuyer(
            [Service] FoodDeliveringContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
            if (user != null)
            {
                var profiles = context.Orders.Where(o => o.UserId == user.Id);
                return profiles.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order> TrackingOrderByBuyer([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.UserName == username).FirstOrDefault();
            if (user != null)
            {
                var orders = context.Orders.Where(o => o.UserId == user.Id).Include(o => o.OrderDetails).OrderBy(o => o.Id).LastOrDefault();
                var latestOrder = context.Orders.Where(o => o.Id == orders.Id);
                return latestOrder.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }

    }
}
