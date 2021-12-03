// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System;
using System.Diagnostics;
using System.IO;
#if NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace Caravela.Framework.Impl.Testing
{
    internal static class CaravelaCompilerUtility
    {
        public static string CompileAssembly( params string[] sourceFiles )
        {
            // TODO: somehow clean up the directory after the test completes?
            var dir = TempPathHelper.GetTempPath( "Tests", Guid.NewGuid() );
            Directory.CreateDirectory( dir );

            void WriteFile( string name, string text ) => File.WriteAllText( Path.Combine( dir, name ), text );

            GlobalJsonWriter.TryWriteCurrentVersion( dir );

            var metadataReader = AssemblyMetadataReader.GetInstance( typeof( CaravelaCompilerUtility ).Assembly );

            var csproj = $@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include='Caravela.Compiler.Sdk' Version='{metadataReader.GetPackageVersion( "Caravela.Compiler.Sdk" )}' />
    <PackageReference Include='Caravela.Compiler' Version='{metadataReader.GetPackageVersion( "Caravela.Compiler" )}' />
  </ItemGroup>
</Project>
";

            WriteFile( "test.csproj", csproj );

            for ( var i = 0; i < sourceFiles.Length; i++ )
            {
                WriteFile( $"file{i}.cs", sourceFiles[i] );
            }

            var psi = new ProcessStartInfo( "dotnet", "build" ) { WorkingDirectory = dir, RedirectStandardOutput = true, UseShellExecute = false };
            var process = Process.Start( psi )!;
            var outputPromise = process.StandardOutput.ReadToEndAsync();

#if NET5_0_OR_GREATER
            var completion = process.WaitForExitAsync();
            Task.WhenAll( completion, outputPromise ).Wait();
#else
            process.WaitForExit();
            outputPromise.Wait();
#endif

            if ( process.ExitCode != 0 )
            {
                throw new InvalidOperationException( outputPromise.Result );
            }

            return Path.Combine( dir, "bin/Debug/net48/test.dll" );
        }
    }
}