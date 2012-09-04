using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Data.Entity;
using System.Net;
using SocialPayments.DomainServices.CustomExceptions;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class BatchesController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/batch
        public HttpResponseMessage<List<BatchModels.BatchResponse>> Get()
        {
            var batchServices = new DomainServices.BatchServices();
            var formattingServices = new DomainServices.FormattingServices();
            List<Domain.TransactionBatch> batches = null;
            HttpResponseMessage<List<BatchModels.BatchResponse>> response = null;

            
            try
            {
                batches = batchServices.GetBatches();
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Batches. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<BatchModels.BatchResponse>>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Batches. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<BatchModels.BatchResponse>>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Batches. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<List<BatchModels.BatchResponse>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;
            }

            response = new HttpResponseMessage<List<BatchModels.BatchResponse>>(batches.Select(b =>
                    new BatchModels.BatchResponse()
                    {
                        ClosedDate = formattingServices.FormatDateTimeForJSON(b.ClosedDate),
                        CreateDate = formattingServices.FormatDateTimeForJSON(b.CreateDate),
                        Id = b.Id,
                        IsClosed = b.IsClosed,
                        LastDateUpdated = formattingServices.FormatDateTimeForJSON(b.LastDateUpdated),
                        TotalDepositAmount = b.TotalDepositAmount,
                        TotalNumberOfDeposits = b.TotalNumberOfDeposits,
                        TotalWithdrawalAmount = b.TotalWithdrawalAmount,
                        TotalNumberOfWithdrawals = b.TotalNumberOfWithdrawals
                    }).ToList(), HttpStatusCode.OK);

            return response;;
        }

        public HttpResponseMessage<BatchModels.BatchResponse> Get(string id)
        {
            var batchServices = new DomainServices.BatchServices();
            var formattingServices = new DomainServices.FormattingServices();
            Domain.TransactionBatch batch = null;
            HttpResponseMessage<BatchModels.BatchResponse> response = null;

            try
            {
                batch = batchServices.GetBatch(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Batch {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<BatchModels.BatchResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Batch {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<BatchModels.BatchResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Batch {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<BatchModels.BatchResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;
            }

            response = new HttpResponseMessage<BatchModels.BatchResponse>(new BatchModels.BatchResponse()
                {
                    ClosedDate = formattingServices.FormatDateTimeForJSON(batch.ClosedDate),
                    CreateDate = formattingServices.FormatDateTimeForJSON(batch.CreateDate),
                    Id = batch.Id,
                    IsClosed = batch.IsClosed,
                    LastDateUpdated = formattingServices.FormatDateTimeForJSON(batch.LastDateUpdated),
                    TotalDepositAmount = batch.TotalDepositAmount,
                    TotalNumberOfDeposits = batch.TotalNumberOfDeposits,
                    TotalWithdrawalAmount = batch.TotalWithdrawalAmount,
                    TotalNumberOfWithdrawals = batch.TotalNumberOfWithdrawals
                }, HttpStatusCode.OK);

            return response;

        }

        // POST /api/batches
        // addes transactions to the current open batch
        public void Post(string value)
        {

        }

        // PUT /api/batch/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/batch/5
        public void Delete(int id)
        {
        }
    }
}
