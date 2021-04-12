using Fission.DotNetCore.Api;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Fission.Dotnet5.Core
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
            return (Task<IActionResult>)_info.Invoke(_assembly.CreateInstance(_type.FullName), new[] { context });
        }

        #endregion

        #region Private/Protectd Methdos

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
