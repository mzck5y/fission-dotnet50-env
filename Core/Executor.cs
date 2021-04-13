using Fission.Dotnet5.Core;
using Fission.DotNetCore.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Oni.Fission.Dotnet5
{
    [Route("/")]
    [ApiController]
    public class ExecutorController : ControllerBase
    {
        #region Fields

        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public ExecutorController(
            IMemoryCache cache,
            ILogger<ExecutorController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        #endregion

        #region Actions

        [HttpPost("specialize")]
        public IActionResult SpecializeFunction()
        {
            string codePath = Environment.GetEnvironmentVariable("FISSION_CODE_PATH") 
                            ?? "/userfunc/user";

            Function func = Function.LoadFunction(codePath); 
            if (func == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            
            _cache.Set("func", func, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });
            
            return Ok();
        }

        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS")]
        public async Task<IActionResult> ExecuteFunction()
        {
            FissionContext ctx = new FissionContext(Request, Response, _logger);

            if (_cache.TryGetValue("func", out Function function) == false)
            {
                _logger.LogCritical("Generic container: no requests supported");
                Console.WriteLine("Generic container: this container does not contantn the fuccion reqeusted OR no requests supported");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
           
            return await function.Invoke(ctx);
        }

        #endregion
    }
}
