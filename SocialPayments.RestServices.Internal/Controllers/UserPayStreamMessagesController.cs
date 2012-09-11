using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using NLog;
using System.Net;
using SocialPayments.Domain;
using SocialPayments.Domain.ExtensionMethods;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPayStreamMessagesController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static DomainServices.FormattingServices _formattingServices = new DomainServices.FormattingServices();

        // GET /api/{userId}/PayStreamMessages
        public HttpResponseMessage<List<MessageModels.MessageResponse>> Get(string userId)
        {
            var userPayStreamMessageServices = new DomainServices.UserPayStreamMessageServices();
            HttpResponseMessage<List<MessageModels.MessageResponse>> response = null;
            List<Domain.Message> messages = null;
            
            try 
            {
                messages = userPayStreamMessageServices.GetPayStreamMessage(userId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting User Paystream Messages for User {0}.  Exception {1}.", userId, ex.Message));

                response = new HttpResponseMessage<List<MessageModels.MessageResponse>>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting User Paystream Messages for User {0}.  Exception {1}.", userId, ex.Message));

                response = new HttpResponseMessage<List<MessageModels.MessageResponse>>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting User Paystream Messages for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<MessageModels.MessageResponse>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            
            response = new HttpResponseMessage<List<MessageModels.MessageResponse>>(messages.Select(m => new MessageModels.MessageResponse()
            {
                amount = m.Amount,
                comments = (!String.IsNullOrEmpty(m.Comments) ? String.Format("{0}", m.Comments) : "No comments"),
                createDate = m.CreateDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                Id = m.Id,
                //lastUpdatedDate =  m.LastUpdatedDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                messageStatus = (m.Direction == "In" ? m.Status.GetRecipientMessageStatus() : m.Status.GetSenderMessageStatus()),
                messageType = m.MessageType.ToString(),
                recipientUri = m.RecipientUri,
                senderUri = m.SenderUri,
                direction = m.Direction,
                latitude = m.Latitude,
                longitutde = m.Longitude,
                senderName = m.SenderName,
                transactionImageUri = m.TransactionImageUrl,
                recipientName = m.RecipientName,
                senderSeen = m.senderHasSeen,
                recipientSeen = m.recipientHasSeen
            }).ToList(), HttpStatusCode.OK);


            return response;
        }

        // GET /api/{userId}/PayStreamMessages/5
        public string Get(int id)
        {
            return "value";
        }

        // POST /api/{userId}/PayStreamMessages
        public void Post(string value)
        {
        }

        // PUT /api/userpaystreammessages/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/userpaystreammessages/5
        public void Delete(int id)
        {
        }
    }
}
