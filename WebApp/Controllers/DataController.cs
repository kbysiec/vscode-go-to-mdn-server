using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Core;
using WebApp.Utils;

namespace WebApp.Controllers
{
    [ApiController]
    public class DataController : ControllerBase
    {
        private Logger Logger { get; }
        private DataService DataService { get; }

        public DataController(Logger logger, DataService dataService)
        {
            Logger = logger;
            DataService = dataService;
        }


        [AllowAnonymous]
        [HttpGet("get")]
        public IActionResult GetData()
        {
            try
            {
                var mdnData = DataService.Get();
                return Ok(mdnData);
            }
            catch (Exception e)
            {
                Logger.Add($@"{e.Message} | STACKTRACE: {e.StackTrace}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize(Policy = "CanAccessPolicy")]
        [HttpGet("count")]
        public IActionResult Count()
        {
            var count = DataService.Count();
            return Ok(count);
        }

        [Authorize(Policy = "CanAccessPolicy")]
        [HttpGet("clear")]
        public IActionResult ClearData()
        {
            DataService.Clear();
            return Ok();
        }

        [Authorize(Policy = "CanAccessPolicy")]
        [HttpGet("get-log")]
        public IActionResult GetLog()
        {
            var logs = Logger.GetAll();
            return Ok(logs);
        }

        [Authorize(Policy = "CanAccessPolicy")]
        [HttpGet("clear-log")]
        public IActionResult ClearLog()
        {
            Logger.ClearAll();
            return Ok();
        }
    }
}
