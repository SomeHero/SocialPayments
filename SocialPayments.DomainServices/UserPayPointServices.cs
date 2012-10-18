using System;
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
                FormattingServices formattingService = new FormattingServices();
                ValidationService validationService = new ValidationService();

                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));
   
                var user = _ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var payPointType = _ctx.PayPointTypes.FirstOrDefault(p => p.Name == payPointTypeName);
                var verified = false;

                if (payPointType == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Pay Point Type {0} not found", uri));

                string unformattedUri = uri;
                if (payPointType.Name == "Phone") {
                        uri = formattingService.RemoveFormattingFromMobileNumber(uri);

                    if (!validationService.IsPhoneNumber(uri))
                        throw new CustomExceptions.BadRequestException(String.Format("{0} is not a valid phone number", unformattedUri));
                }
                if (payPointType.Name == "EmailAddress")
                {
                    if (!validationService.IsEmailAddress(uri))
                        throw new CustomExceptions.BadRequestException(String.Format("{0} is not a valid email address", unformattedUri));
                }
                if (payPointType.Name == "MeCode")
                {
                    verified = true;

                    if (!validationService.IsMECode(uri))
                        throw new CustomExceptions.BadRequestException(String.Format("{0} is not a valid Me Code", unformattedUri));
                }

                var payPoints = _ctx.UserPayPoints.FirstOrDefault(p => p.URI == uri);

                if (payPoints != null)
                    throw new CustomExceptions.BadRequestException(String.Format("The pay point {0} is already linked to an account", uri));

                //TODO: Validate format of the URI based on type

                var userPayPoint = _ctx.UserPayPoints.Add(new Domain.UserPayPoint()
                {
                    Id = Guid.NewGuid(),
                    User = user,
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    URI = uri,
                    Type = payPointType,
                    Verified = verified
                });

                var messages = _ctx.Messages
                    .Where(m => m.RecipientUri == uri && (m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.SubmittedRequest)
                        || m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.NotifiedRequest)
                        || m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.SubmittedPayment)
                        || m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.NotifiedPayment)));

                foreach(var message in messages)
                {
                    message.Recipient = user;

                    switch (message.MessageType)
                    {
                        case Domain.MessageType.Payment:
                            message.Status = Domain.PaystreamMessageStatus.ProcessingPayment;
                            break;
                        case Domain.MessageType.PaymentRequest:
                            message.Status = Domain.PaystreamMessageStatus.PendingRequest;
                            break;
                    }
                }

                _ctx.SaveChanges();

                if (payPointType.Name == "EmailAddress")
                    userService.SendEmailVerificationLink(userId, userPayPoint.Id.ToString());
                else if (payPointType.Name == "Phone")
                    userService.SendMobileVerificationCode(userId, userPayPoint.Id.ToString());


                return userPayPoint;
            }
        }
        public List<Domain.UserPayPoint> GetUserPayPoints(string userId, string type)
        {
            using (var _ctx = new Context())
            {
                var userService = new UserService();
                var validationService = new DomainServices.ValidationService();
                var formattingServices = new DomainServices.FormattingServices();
                
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

                foreach (var payPoint in payPoints)
                {
                    if (payPoint.PayPointTypeId == 2 && validationService.IsPhoneNumber(payPoint.URI))
                        payPoint.URI = formattingServices.FormatMobileNumber(payPoint.URI);
                }

                return payPoints;

            }
        }
        public Domain.UserPayPoint GetUserPayPoint(string userId, string id)
        {
            using (var _ctx = new Context())
            {
                var validationService = new DomainServices.ValidationService();
                var formattingServices = new DomainServices.FormattingServices();
                
                UserService userService = new UserService(_ctx);

                var user = userService.GetUserById(userId);
                Guid payPointId;

                Guid.TryParse(id, out payPointId);

                var payPoint = _ctx.UserPayPoints
                    .Include("Type")
                    .FirstOrDefault(p => p.UserId == user.UserId && p.Id == payPointId);

                if (payPoint.PayPointTypeId == 2 && validationService.IsPhoneNumber(payPoint.URI))
                    payPoint.URI = formattingServices.FormatMobileNumber(payPoint.URI);

                return payPoint;
            }
            
        }
        public void DeleteUserPayPoint(string userId, string userPayPointId)
        {
            using (var _ctx = new Context())
            {
                UserService userService = new UserService();
                UserPayPointServices userPayPointServices = new UserPayPointServices();
                Domain.UserPayPoint userPayPoint = new Domain.UserPayPoint();

                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var user = _ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                Guid userPayPointGuid;
                Guid.TryParse(userPayPointId, out userPayPointGuid);

                if (userPayPointGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Pay Point {0} Not Found", userPayPointId));

                userPayPoint = _ctx.UserPayPoints
                    .FirstOrDefault(p => p.UserId == user.UserId && p.Id == userPayPointGuid);

                if (userPayPoint == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Pay Point {0} Not Found", userPayPointId));

                var userPayPointHistory = _ctx.UserPayPointHistory.Add(new Domain.UserPayPointHistory()
                {
                    Id = Guid.NewGuid(),
                    PayPointURI = userPayPoint.URI,
                    StartDate = userPayPoint.CreateDate,
                    EndDate = System.DateTime.Now,
                    UserId = userPayPoint.UserId
                });

                _ctx.UserPayPoints.Remove(userPayPoint);

                _ctx.SaveChanges();
            }
        }
        public void AddMobileNumberSignUp(string signUpKey, string mobileNumber)
        {
            using (var ctx = new Context())
            {
                DomainServices.UserService userService = new DomainServices.UserService(ctx);

                Guid userId;

                Guid.TryParse(signUpKey, out userId);

                if (userId == null)
                    throw new CustomExceptions.BadRequestException(String.Format("User {0} Not Found", mobileNumber));

                Domain.User user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (user == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Exception Process Registration SMS Signup for user {0}", signUpKey));

                user.MobileNumber = mobileNumber;

                user.PayPoints.Add(new Domain.UserPayPoint()
                {
                    CreateDate = System.DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    IsActive = true,
                    PayPointTypeId = 2,
                    URI = mobileNumber,
                    Verified = true,
                    VerifiedDate = System.DateTime.UtcNow
                });

                var mobileNumberAttribute = ctx.UserAttributes
                   .FirstOrDefault(a => a.AttributeName == "phoneUserAttribute");

                if (mobileNumberAttribute != null)
                {
                    user.UserAttributes.Add(new Domain.UserAttributeValue()
                    {
                        id = Guid.NewGuid(),
                        UserId = user.UserId,
                        UserAttributeId = mobileNumberAttribute.Id,
                        AttributeValue = mobileNumber
                    });
                }

                ctx.SaveChanges();

            }
        }
    }
}
