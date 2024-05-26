using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

namespace MrAnonCrypter
{
  internal class Compiler
  {
    public static byte[] Build(string source, string[] references)
    {
      using (CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider())
      {
        CompilerParameters options = new CompilerParameters(references, AppDomain.CurrentDomain.BaseDirectory + "bin\\" + Randomiser.GetString(6) + ".tmp")
        {
          GenerateExecutable = true,
          CompilerOptions = "-optimize -unsafe -platform:anycpu",
          IncludeDebugInformation = false
        };
        CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromSource(options, source);
        if (compilerResults.Errors.Count > 0)
          throw new CompilerErrorException(string.Join(Environment.NewLine, compilerResults.Errors.Cast<CompilerError>().Select<CompilerError, string>((Func<CompilerError, string>) (s => s.ToString()))));
        byte[] numArray = File.ReadAllBytes(compilerResults.PathToAssembly);
        File.Delete(compilerResults.PathToAssembly);
        return numArray;
      }
    }
  }
}