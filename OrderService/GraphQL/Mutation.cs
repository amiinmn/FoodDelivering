using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using System;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using OrderService.Models;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Courier> AddCourierAsync(
            CourierInput input,
            [Service] FoodDeliveringContext context)
        {

            // EF
            var courier = new Courier
            {
                Name = input.Name,
                Phone = input.Phone,
                Status = Convert.ToBoolean(input.Status)

            };

            var ret = context.Couriers.Add(courier);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<OrderData> AddOrderAsync(
            OrderData input,
            ClaimsPrincipal claimsPrincipal,
            [Service] FoodDeliveringContext context)
        {
            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;

            try
            {
                var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
                if (user != null)
                {
                    // EF
                    var order = new Order
                    {
                        Code = Guid.NewGuid().ToString(), // generate random chars using GUID
                        UserId = user.Id,
                        CourierId = user.Id
                    };
                                     
                    foreach (var item in input.Details)
                    {
                        var detial = new OrderDetail
                        {
                            OrderId = order.Id,
                            FoodId = item.FoodId,
                            Quantity = item.Quantity
                        };
                        order.OrderDetails.Add(detial);            
                    }
                    context.Orders.Add(order);
                    context.SaveChanges();
                    await transaction.CommitAsync();

                    input.Id = order.Id;
                    input.Code = order.Code;
                }
                else
                    throw new Exception("user was not found");
            }
            catch(Exception err)
            {
                transaction.Rollback();
            }



            return input;
        }

    }
}
