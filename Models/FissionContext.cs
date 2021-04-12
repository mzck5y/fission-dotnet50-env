using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Fission.DotNetCore.Api
{
    public class FissionContext
    {
        #region Properties

        public string PackagePath { get; set; } = string.Empty;

        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
        public ILogger Logger { get; set; }

        #endregion

        #region Constructors

        public FissionContext(
            HttpRequest request, 
            HttpResponse response, 
            IDictionary<string, object> arguments, 
            ILogger logger)
        {
            Request = request;
            Response = response;
            Arguments = arguments;
            Logger = logger;
        }

        #endregion

        #region Public Methods

        public static FissionContext Build(HttpRequest request, HttpResponse response, IDictionary<string, object> arguments, ILogger logger)
        {
            return new FissionContext(request, response, arguments, logger);
        }

        public string GetSettingsJson(string relativePath)
        {
            return File.ReadAllText(Path.Combine(PackagePath, relativePath));
        }

        public T GetSettings<T>(string relativePath)
        {
            string fullPath = Path.Combine(PackagePath, relativePath);
            Logger.LogInformation("About to get settngs file from {0}", fullPath);
            string json = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        #endregion
    }
}
