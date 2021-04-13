using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fission.DotNetCore.Core
{
    public class FissionContext
    {
        #region Properties

        public string PackagePath { get; set; } = string.Empty;
        
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public IDictionary<string, object> Arguments { get; private set; } = new Dictionary<string, object>();
        public ILogger Logger { get; set; }

        #endregion

        #region Constructors

        public FissionContext(
            HttpRequest request, 
            HttpResponse response,
            ILogger logger)
        {
            Request = request;
            Response = response;
            Logger = logger;
        }

        #endregion

        #region Public Methods

        public static FissionContext Build(HttpRequest request, HttpResponse response, ILogger logger)
        {
            return new FissionContext(request, response, logger);
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
            
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T> GetBody<T>()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            string body = await sr.ReadToEndAsync();

            if (string.IsNullOrEmpty(body))
            {
                return default;
            }

            T item = JsonSerializer.Deserialize<T>(body);

            return item;
        }

        #endregion
    }
}
