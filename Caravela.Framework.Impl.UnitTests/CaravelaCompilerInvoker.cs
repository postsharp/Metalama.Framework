using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    class CaravelaCompiler
    {
        public static string CompileAssembly(params string[] sourceFiles)
        {
            // TODO: somehow clean up the directory after the test completes?
            var dir = Path.Combine( Path.GetTempPath(), "CaravelaTests", Guid.NewGuid().ToString() )!;
            Directory.CreateDirectory( dir );

            void WriteFile( string name, string text ) => File.WriteAllText( Path.Combine( dir, name ), text );

            // TODO: use WritePackageVersions from Caravela.Framework.Impl.csproj to avoid hardcoding the version?
            string csproj = @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include='Caravela.Compiler.Sdk' Version='0.1.51' />
    <PackageReference Include='Caravela.Compiler' Version='0.1.51' />
  </ItemGroup>
</Project>
";

            WriteFile( "test.csproj", csproj );

            for ( int i = 0; i < sourceFiles.Length; i++ )
            {
                WriteFile( $"file{i}.cs", sourceFiles[i] );
            }

            var psi = new ProcessStartInfo( "dotnet", "build" ) 
            {
                WorkingDirectory = dir, 
                RedirectStandardOutput = true
            };
            var process = Process.Start( psi )!;
            process.WaitForExit();

            Assert.True( process.ExitCode == 0, process.StandardOutput.ReadToEnd() );

            return Path.Combine( dir, "bin/Debug/net5.0/test.dll" );
        }
    }
}
