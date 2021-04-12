using Fission.DotNetCore.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

public class FissionFunction
{
    public async Task<IActionResult> Execute(FissionContext ctx)
    {
        ctx.Logger.LogInformation("C# dotnet 5 function.");

        string name = ctx.Request.Query["name"];
        using StreamReader sr = new StreamReader(ctx.Request.Body);
        string body = await sr.ReadToEndAsync();

        ctx.Logger.LogInformation($"Request Body: {body}");

        // ctx.Response.StatusCode = 201;
        ctx.Response.Headers.Add("X-FISSION-ID", $"123456abc {Guid.NewGuid().ToString()}");

        return name != null
            ? (ActionResult)new OkObjectResult($"Hello, {name}")
            : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
    }
}