﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class UserPayPointServices
    {
        public Domain.UserPayPoint AddUserPayPoint(string userId, string payPointTypeName, string uri)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found"));

                var payPointType = _ctx.PayPointTypes.FirstOrDefault(p => p.Name == payPointTypeName);

                if (payPointType == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Pay Point Type {0} not found", payPointType));

                var payPoints = _ctx.UserPayPoints.FirstOrDefault(p => p.URI == uri);

                if (payPoints != null)
                    throw new CustomExceptions.BadRequestException(String.Format("The pay point {0} is already linked to an account", payPointType));

                //TODO: Validate format of the URI based on type

                var userPayPoint = _ctx.UserPayPoints.Add(new Domain.UserPayPoint()
                {
                    Id = Guid.NewGuid(),
                    User = user,
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    URI = uri,
                    Type = payPointType,
                    Verified = false
                });

                _ctx.SaveChanges();

                if (payPointType.Name == "EmailAddress")
                    userService.SendEmailVerificationLink(userPayPoint);
                else if (payPointType.Name == "Phone")
                    userService.SendMobileVerificationCode(userPayPoint);


                return userPayPoint;
            }
        }
        public List<Domain.UserPayPoint> GetUserPayPoints(string userId, string type)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService();
                List<Domain.UserPayPoint> payPoints;

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                if (!String.IsNullOrEmpty(type))
                {
                    int typeId = 0;

                    if (type == "EmailAddress")
                        typeId = 1;
                    else if (type == "Phone")
                        typeId = 2;
                    else if (type == "MeCode")
                        typeId = 4;

                    payPoints = _ctx.UserPayPoints
                    .Include("Type")
                    .Where(p => p.UserId == user.UserId && p.IsActive && p.PayPointTypeId == typeId)
                    .Select(p => p)
                    .ToList<Domain.UserPayPoint>();
                }
                else
                {
                    payPoints = _ctx.UserPayPoints
                    .Include("Type")
                    .Where(p => p.UserId == user.UserId && p.IsActive)
                    .Select(p => p)
                    .ToList<Domain.UserPayPoint>();
                }

                return payPoints;

            }
        }
        public Domain.UserPayPoint GetUserPayPoint(string userId, string id)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService(_ctx);

                var user = userService.GetUserById(userId);
                Guid payPointId;

                Guid.TryParse(id, out payPointId);

                var payPoint = _ctx.UserPayPoints
                    .Include("Type")
                    .FirstOrDefault(p => p.UserId == user.UserId && p.Id == payPointId);

                return payPoint;
            }
            
        }
        public void DeleteUserPayPoint(string userId, string userPayPointId)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService();
                UserPayPointServices userPayPointServices = new UserPayPointServices();
                Domain.User user = new Domain.User();
                Domain.UserPayPoint userPayPoint = new Domain.UserPayPoint();

                user = userService.GetUser(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                userPayPoint = GetUserPayPoint(userId, userPayPointId);

                if (userPayPoint == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Pay Point {0} Not Found", userPayPointId));

                userPayPoint.IsActive = false;

                _ctx.SaveChanges();
            }
        }
    }
}