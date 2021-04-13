using Fission.DotNetCore.Attributes;
using Fission.DotNetCore.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fission.DotNetCore
{
    public class FissionFunction
    {
        [HttpPost]
        [FunctionName("first-function")]
        [HmacSha1("hmac", "3A6AD150A24C47D190D7EE3FF5CECDEEC1230C335FFC437F9AA44A575D7F0D55")]
        public async Task<IActionResult> Execute(FissionContext ctx)
        {
            ctx.Logger.LogInformation("C# dotnet 5 function.");

            string name = ctx.Request.Query["name"];

            Person person = await ctx.GetBody<Person>();
            if (person == null)
            {
                return new BadRequestObjectResult("Function body is null or empty");
            }
            person.FirstName = name;

            // ctx.Response.StatusCode = 201;
            ctx.Response.Headers.Add("X-FISSION-ID", $"{Guid.NewGuid().ToString()}");

            return name != null
                ? (ActionResult)new OkObjectResult(person)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

    public class Person
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }
}