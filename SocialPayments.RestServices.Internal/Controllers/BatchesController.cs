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
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var batchServices = new DomainServices.BatchServices();
            var formattingServices = new DomainServices.FormattingServices();
            List<Domain.TransactionBatch> batches = null;
            HttpResponseMessage response = null;

            
            try
            {
                batches = batchServices.GetBatches();
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Batches. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Batches. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));
                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Batches. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            response = Request.CreateResponse(HttpStatusCode.OK, batches.Select(b =>
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
                    }).ToList());

            return response;;
        }

        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            var batchServices = new DomainServices.BatchServices();
            var formattingServices = new DomainServices.FormattingServices();
            Domain.TransactionBatch batch = null;
            HttpResponseMessage response = null;

            try
            {
                batch = batchServices.GetBatch(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Batch {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
  
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Batch {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Batch {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            response = Request.CreateResponse(HttpStatusCode.OK, new BatchModels.BatchResponse()
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
                });

            return response;

        }
    }
}
