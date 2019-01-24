using FileDownloader.Core.Dtos.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace FileDownloader.Web.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> _logger;

        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid == false)
            {
                string errorDescription = string.Empty;

                ModelErrorCollection modelErrorCollection = context.ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).FirstOrDefault();

                if (modelErrorCollection != null)
                {
                    ModelError modelError = modelErrorCollection.FirstOrDefault();

                    if (modelError != null)
                    {
                        errorDescription = modelError.ErrorMessage;
                    }
                }

                if (string.IsNullOrEmpty(errorDescription))
                    errorDescription = "Invalid model";

                _logger.LogDebug("Modal validation failed URL: " + context.HttpContext.Request.Path + " Errors: " + errorDescription);

                context.Result = new BadRequestObjectResult(new ApiErrorDto() { Code = "invalid_model", Message = errorDescription });
            }
        }
    }
}