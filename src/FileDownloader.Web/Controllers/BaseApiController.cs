using FileDownloader.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FileDownloader.Web.Controllers
{
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ExceptionFilterAttribute))]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class BaseApiController : Controller
    {
    }
}