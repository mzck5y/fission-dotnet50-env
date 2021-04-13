using Fission.DotNetCore.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Fission.DotNetCore.Core
{
    public class Function : AssemblyLoadContext
    {
        #region Fields

        private readonly Assembly _assembly;
        private readonly Type _type;
        private readonly MethodInfo _info;

        #endregion

        #region Constructors

        private Function(byte[] compiledAsm)
            : base(true)
        {
            using Stream stream = new MemoryStream(compiledAsm);

            _assembly = LoadFromStream(stream);
            _type = _assembly.GetTypes().FirstOrDefault(t => t.Name.EndsWith("FissionFunction"));
            _info = (MethodInfo)_type.GetMember("Execute").First();
        }

        #endregion

        #region Public Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Function LoadFunction(string codePath)
        {
            try
            {
                // Get function source code
                string code = GetSourceCode(codePath);
                if (string.IsNullOrEmpty(code))
                {
                    Console.WriteLine($"Souce code at {codePath} is null or empty");
                }

                // Compile source code
                byte[] compiledAssembly = Compiler.CompileFunction(code);
                if (compiledAssembly == null)
                {
                    Console.WriteLine($"Unable to compile code");
                }
                
                return new Function(compiledAssembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine("an exception happen {0}", ex.Message);
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task<IActionResult> Invoke(FissionContext context)
        {
            if (ValidateMethod(context.Request.Method) == false)
            {
                return Task.FromResult((IActionResult)new StatusCodeResult((int)HttpStatusCode.MethodNotAllowed));
            }

            if (ValidateHmacSha1SignatureAsync(context).Result == false)
            {
                return Task.FromResult((IActionResult)new StatusCodeResult((int)HttpStatusCode.Forbidden));
            }

            string funcName = _info.GetCustomAttribute<FunctionNameAttribute>()?.Name;
            context.Logger.LogInformation($"Function Name Called: {funcName}");

            return (Task<IActionResult>)_info.Invoke(_assembly.CreateInstance(_type.FullName), new[] { context });
        }

        #endregion

        #region Private/Protectd Methdos

        private bool ValidateMethod(string requestMethod)
        {
            HttpMethodAttribute attr = (HttpMethodAttribute)_info.GetCustomAttributes(
                    typeof(HttpMethodAttribute), true).FirstOrDefault();

            string method = attr?.HttpMethods.FirstOrDefault() ?? "POST";

            return string.Compare(method, requestMethod, true) == 0;
        }

        private async Task<bool> ValidateHmacSha1SignatureAsync(FissionContext context)
        {
            HmacSha1Attribute attr = _info.GetCustomAttribute<HmacSha1Attribute>(true);
            if (attr != null)
            {
                return await attr.IsSignatureValidAsync(context);
            }

            return true; // if attr is null then no hmacsha1 validation was specified
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetSourceCode(string codePath)
        {
            if (File.Exists(codePath) == false)
            {
                Console.WriteLine($"Unable to loacate function code at: {codePath}");
                return null;
            }

            return File.ReadAllText(codePath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }

        #endregion
    }
}
