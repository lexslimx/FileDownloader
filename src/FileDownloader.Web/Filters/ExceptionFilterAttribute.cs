using FileDownloader.Core.Dtos.Shared;
using FileDownloader.Core.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace FileDownloader.Web.Filters
{
    public class ExceptionFilterAttribute : IExceptionFilter
    {
        private ILogger<ExceptionFilterAttribute> _logger;

        public ExceptionFilterAttribute(ILogger<ExceptionFilterAttribute> logger, IHostingEnvironment env)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Application thrown error");

            HttpStatusCode status = HttpStatusCode.BadRequest;

            string code = string.Empty;
            string message = context.Exception.Message;

            Type exceptionType = context.Exception.GetType();

            if (exceptionType == typeof(CustomException))
            {
                CustomException customException = (CustomException)context.Exception;

                code = customException.Code;
                status = customException.HttpStatusCode;
            }
            else
            {
                code = "server_error";
            }

            context.Result = new JsonResult(new ApiErrorDto() { Code = code, Message = message });

            context.ExceptionHandled = true;

            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = (int)status;
            response.ContentType = "application/json";
        }
    }
}