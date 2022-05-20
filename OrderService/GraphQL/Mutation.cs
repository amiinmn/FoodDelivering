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

        //[Authorize(Roles = new[] { "BUYER" })]
        //public async Task<OrderData> AddOrderAsync(
        //OrderData input,
        //ClaimsPrincipal claimsPrincipal,
        //[Service] FoodDeliveringContext context)
        //{
        //    using var transaction = context.Database.BeginTransaction();
        //    var userName = claimsPrincipal.Identity.Name;
        //    try
        //    {
        //        var user = context.Users.Where(o => o.UserName == userName).FirstOrDefault();
        //        if (user != null)
        //        {
        //            // EF
        //            var order = new Order
        //            {
        //                Code = Guid.NewGuid().ToString(), // generate random chars using GUID
        //                UserId = user.Id,
        //                CourierId = input.CourierId,
        //                Longitude = input.Longitude,
        //                Latitude = input.Latitude
        //            };

        //            foreach (var item in input.Details)
        //            {
        //                var detail = new OrderDetail
        //                {
        //                    OrderId = order.Id,
        //                    FoodId = item.FoodId,
        //                    Quantity = item.Quantity
        //                };
        //                order.OrderDetails.Add(detail);
        //            }
        //            context.Orders.Add(order);
        //            context.SaveChanges();
        //            await transaction.CommitAsync();

        //            //input.Id = order.Id;
        //            //input.Code = order.Code;
        //        }
        //        else
        //            throw new Exception("user was not found");
        //    }
        //    catch (Exception err)
        //    {
        //        transaction.Rollback();
        //    }

        //    return input;
        //}

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
                var courier = context.Couriers.Where(o => o.Id == input.CourierId).FirstOrDefault();
                if (user != null)
                {
                    // EF
                    if (courier.Status == "AVAILABLE")
                    {
                        var order = new Order
                        {
                            Code = Guid.NewGuid().ToString(), // generate random chars using GUID
                            UserId = user.Id,
                            CourierId = input.CourierId
                        };

                        foreach (var item in input.Details)
                        {
                            var detail = new OrderDetail
                            {
                                OrderId = order.Id,
                                FoodId = item.FoodId,
                                Quantity = item.Quantity
                            };
                            order.OrderDetails.Add(detail);
                        }
                        context.Orders.Add(order);

                        courier.Status = "PROGRESS";
                        context.Couriers.Update(courier);

                        context.SaveChanges();
                        await transaction.CommitAsync();
                    }

                    //input.Code = order.Code;
                }
                else
                    throw new Exception("user was not found");
            }
            catch (Exception err)
            {
                transaction.Rollback();
            }

            return input;
        }


        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Order> UpdateOrderAsync(
            OrderData input,
            [Service] FoodDeliveringContext context)
        {
            var user = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.Code = Guid.NewGuid().ToString();
                user.UserId = input.UserId;
                user.CourierId = input.CourierId;


                context.Orders.Update(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<Order> DeleteOrderByIdAsync(
        //    int id,
        //    [Service] FoodDeliveringContext context)
        //{
        //    var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
        //    if (order != null)
        //    {
        //        context.Orders.Remove(order);
        //        await context.SaveChangesAsync();
        //    }

        //    return await Task.FromResult(order);
        //}
        

        //===============ADD TRACKING BY COURIER===============================//
        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<Tracking> AddTrackingOrderAsync(
            Tracking input,
            [Service] FoodDeliveringContext context)
        {
            var order = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();
            if (order != null)
            {
                // EF
                order.Id = input.Id;
                order.Longitude = input.Longitude;
                order.Latitude = input.Latitude;

                context.Orders.Update(order);
                context.SaveChanges();
            }
            return input;
        }

        //===========================COMPLETE ORDER=======================================//
        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<Order> CompleteOrderAsync(
            int id,
            [Service] FoodDeliveringContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            var courier = context.Couriers.Where(o => o.Id == order.CourierId).FirstOrDefault();
            if (order != null)
            {
                // EF
                order.Id = id;
                courier.Status = "AVAILABLE";
                context.Couriers.Update(courier);
                context.SaveChanges();
            }
            return await Task.FromResult(order);
        }

    }
}
