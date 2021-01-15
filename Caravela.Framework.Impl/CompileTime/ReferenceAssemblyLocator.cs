using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static Caravela.Framework.Impl.CompileTime.PackageVersions;

namespace Caravela.Framework.Impl.CompileTime
{
    static class ReferenceAssemblyLocator
    {
        private static readonly string project = $@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.CSharp' Version='{MicrosoftCSharpVersion}' />
    <PackageReference Include='Microsoft.CodeAnalysis.CSharp' Version='{MicrosoftCodeAnalysisCSharpVersion}' />
  </ItemGroup>
  <Target Name='WriteReferenceAssemblies' DependsOnTargets='FindReferenceAssembliesForReferences'>
    <WriteLinesToFile File='assemblies.txt' Overwrite='true' Lines='@(ReferencePathWithRefAssemblies)' />
  </Target>
</Project>";

        public static IEnumerable<string> GetReferenceAssemblies()
        {
            string hash = ComputeHash( project );
            string tempProjectDirectory = Path.Combine( Path.GetTempPath(), "Caravela", hash, "TempProject" );

            string referenceAssemlyListFile = Path.Combine( tempProjectDirectory, "assemblies.txt" );

            if ( File.Exists( referenceAssemlyListFile ) )
            {
                var referenceAssemblies = File.ReadAllLines( referenceAssemlyListFile );

                if ( referenceAssemblies.All( File.Exists ) )
                    return referenceAssemblies;
            }

            Directory.CreateDirectory( tempProjectDirectory );

            File.WriteAllText( Path.Combine( tempProjectDirectory, "TempProject.csproj" ), project );

            var psi = new ProcessStartInfo( "dotnet", "build -t:WriteReferenceAssemblies" )
            {
                WorkingDirectory = tempProjectDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            var process = Process.Start( psi );

            var lines = new List<string>();
            process.OutputDataReceived += ( _, e ) => lines.Add( e.Data );

            process.BeginOutputReadLine();
            process.WaitForExit();

            if ( process.ExitCode != 0 )
                throw new InvalidOperationException( "Error while building temporary project to locate reference assemblies:" + Environment.NewLine + string.Join( Environment.NewLine, lines ) );

            return File.ReadAllLines( referenceAssemlyListFile );
        }

        private static string ComputeHash(string input)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash( Encoding.UTF8.GetBytes( input ) );
            return BitConverter.ToString( hash ).Replace( "-", "" );
        }
    }
}
