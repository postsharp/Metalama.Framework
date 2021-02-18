using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Caravela.Framework.Impl.CompileTime;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    internal class CaravelaCompiler
    {
        public static string CompileAssembly( params string[] sourceFiles )
        {
            // TODO: somehow clean up the directory after the test completes?
            var dir = Path.Combine( Path.GetTempPath(), "CaravelaTests", Guid.NewGuid().ToString() )!;
            Directory.CreateDirectory( dir );

            void WriteFile( string name, string text ) => File.WriteAllText( Path.Combine( dir, name ), text );

            var csproj = $@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include='Caravela.Compiler.Sdk' Version='{PackageVersions.CaravelaCompilerSdkVersion}' />
    <PackageReference Include='Caravela.Compiler' Version='{PackageVersions.CaravelaCompilerVersion}' />
  </ItemGroup>
</Project>
";

            WriteFile( "test.csproj", csproj );

            for ( var i = 0; i < sourceFiles.Length; i++ )
            {
                WriteFile( $"file{i}.cs", sourceFiles[i] );
            }

            var psi = new ProcessStartInfo( "dotnet", "build" )
            {
                WorkingDirectory = dir,
                RedirectStandardOutput = true
            };
            var process = Process.Start( psi )!;
            var completion = process.WaitForExitAsync();
            var outputPromise = process.StandardOutput.ReadToEndAsync();

            Task.WhenAll( completion, outputPromise ).Wait();

            Xunit.Assert.True( process.ExitCode == 0, outputPromise.Result );

            return Path.Combine( dir, "bin/Debug/net48/test.dll" );
        }
    }
}
