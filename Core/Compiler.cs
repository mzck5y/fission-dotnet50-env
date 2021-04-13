using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Primitives;  
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fission.DotNetCore.Core
{
    public class Compiler
    {
        #region Public Methods

        public static byte[] CompileFunction(string sourceCode)
        {
            Console.WriteLine("Starting the compilation");

            // string sourceCode = File.ReadAllText(filePath);

            using MemoryStream strm = new MemoryStream();
            EmitResult result = GenerateCode(sourceCode).Emit(strm);
            if (result.Success == false)
            {
                Console.WriteLine("Compilation Errors Found");
                IEnumerable<Diagnostic> failuers = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);
                foreach (var d in failuers)
                {
                    Console.WriteLine("{0}: {1}", d.Id, d.GetMessage());
                }

                return null;
            }

            Console.WriteLine("Compilation Successfull");

            strm.Seek(0, SeekOrigin.Begin);

            return strm.ToArray();
        }

        #endregion

        #region Private Methods

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            SourceText code = SourceText.From(sourceCode);
            CSharpParseOptions options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
            
            SyntaxTree parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(code, options);
            
            DirectoryInfo coreDir = Directory.GetParent(typeof(Enumerable).GetTypeInfo().Assembly.Location);
            
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.GetEntryAssembly().Location),
                MetadataReference.CreateFromFile(typeof(IHeaderDictionary).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StringValues).Assembly.Location)
            };

            Assembly.GetEntryAssembly()?.GetReferencedAssemblies().ToList()
               .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

            string asmName = Path.GetRandomFileName();

            return CSharpCompilation.Create($"{asmName}.dll",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        #endregion
    }
}
