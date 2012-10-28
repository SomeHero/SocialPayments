using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Data.Entity;
using NLog;
using System.Threading.Tasks;
using SocialPayments.DomainServices.UserPayPointProcessing;

namespace SocialPayments.DomainServices
{
    public class UserSocialNetworkServices
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void AddUserSocialNetwork(string userId, string socialNetworkName, string socialNetworkUserId, string socialNetworkToken)
        {
            using (var ctx = new Context())
            {
                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userGuid);

                if(user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var socialNetwork = ctx.SocialNetworks.FirstOrDefault(s => s.Name == socialNetworkName);

                if (socialNetwork == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Social Network {0} Not Valid", socialNetworkName));

                var uri = String.Format("fb_{0}", socialNetworkUserId);

                var payPoints = ctx.UserPayPoints.FirstOrDefault(p => p.URI == uri);

                if (payPoints != null)
                    throw new CustomExceptions.BadRequestException(String.Format("The social network is already linked to an account", uri));


                var userSocialNetwork = ctx.UserSocialNetworks.Add(new Domain.UserSocialNetwork()
                {
                    EnableSharing = false,
                    SocialNetwork = socialNetwork,
                    User = user,
                    UserNetworkId = socialNetworkUserId,
                    UserAccessToken = socialNetworkToken
                });


                var payPointType = ctx.PayPointTypes.FirstOrDefault(p => p.Name == socialNetworkName);
                var verified = true;

                if (payPointType == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Pay Point Type {0} not found", uri));

                var userPayPoint = ctx.UserPayPoints.Add(new Domain.UserPayPoint()
                {
                    Id = Guid.NewGuid(),
                    User = user,
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    URI = uri,
                    Type = payPointType,
                    Verified = verified
                });

                var messages = ctx.Messages
                   .Where(m => m.RecipientUri == userPayPoint.URI && (m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.SubmittedRequest)
                       || m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.NotifiedRequest)
                       || m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.SubmittedPayment)
                       || m.StatusValue.Equals((int)Domain.PaystreamMessageStatus.NotifiedPayment)));

                foreach (var message in messages)
                {
                    message.Recipient = userPayPoint.User;

                    switch (message.MessageType)
                    {
                        case Domain.MessageType.Payment:
                            message.Status = Domain.PaystreamMessageStatus.PendingPayment;
                            break;
                        case Domain.MessageType.PaymentRequest:
                            message.Status = Domain.PaystreamMessageStatus.PendingRequest;
                            break;
                    }
                }


                ctx.SaveChanges();

                Task.Factory.StartNew(() =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Started Added PayPoint Task. {0} to {1}", user.UserName, userPayPoint.Id));

                    AddUserPayPointTask task = new AddUserPayPointTask();
                    task.Excecute(userPayPoint.Id);

                }).ContinueWith(task =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Completed Added PayPoint Task. {0} to {1}", user.UserName, userPayPoint.Id));
                });
            }
        }
        public void DeleteUserSocialNetwork(string userId, string socialNetworkName)
        {
            using (var ctx = new Context())
            {
                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var user = ctx.Users
                    .Include("UserSocialNetworks")
                    .Include("UserSocialNetworks.SocialNetwork")
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var socialNetwork = user.UserSocialNetworks.FirstOrDefault(n => n.SocialNetwork.Name == socialNetworkName);

                if (socialNetwork == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Social Network {0} Not Valid", socialNetworkName));

                ctx.UserSocialNetworks.Remove(socialNetwork);

                var uri = String.Format("fb_{0}", socialNetwork.UserNetworkId);

                var userPayPoint = ctx.UserPayPoints
                   .FirstOrDefault(p => p.UserId == user.UserId && p.URI == uri);

                if (userPayPoint != null)
                {

                    var userPayPointHistory = ctx.UserPayPointHistory.Add(new Domain.UserPayPointHistory()
                    {
                        Id = Guid.NewGuid(),
                        PayPointURI = userPayPoint.URI,
                        StartDate = userPayPoint.CreateDate,
                        EndDate = System.DateTime.Now,
                        UserId = userPayPoint.UserId
                    });

                    ctx.UserPayPoints.Remove(userPayPoint);
                }

                ctx.SaveChanges();
            }
        }
    }
}
