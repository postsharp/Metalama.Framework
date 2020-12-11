using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static Caravela.Framework.Impl.CompileTime.PackageVersions;

namespace Caravela.Framework.Impl.CompileTime
{
    public static class ReferenceAssemblyLocator
    {
        public static IEnumerable<string> GetReferenceAssemblies()
        {
            string version = typeof( CompileTimeAssemblyBuilder ).Assembly.GetName().Version.ToString();
            string tempProjectDirectory = Path.Combine( Path.GetTempPath(), "Caravela", version, "TempProject" );

            string referenceAssemlyListFile = Path.Combine( tempProjectDirectory, "assemblies.txt" );

            if ( File.Exists( referenceAssemlyListFile ) )
            {
                return File.ReadAllLines( referenceAssemlyListFile );
            }

            Directory.CreateDirectory( tempProjectDirectory );

            string project = $@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.CSharp' Version='{MicrosoftCSharpVersion}' />
    <PackageReference Include='Microsoft.CodeAnalysis.CSharp' Version='{MicrosoftCodeAnalysisCSharpVersion}' />
  </ItemGroup>
</Project>";
            File.WriteAllText( Path.Combine( tempProjectDirectory, "TempProject.csproj" ), project );

            var psi = new ProcessStartInfo( "dotnet", "build -t:rebuild -clp:ShowCommandLine" )
            {
                WorkingDirectory = tempProjectDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            var process = Process.Start( psi );

            var lines = new List<string>();
            process.OutputDataReceived += ( _, e ) => lines.Add( e.Data );

            process.BeginOutputReadLine();
            process.WaitForExit();

            if ( process.ExitCode != 0 )
                throw new InvalidOperationException( "Error while building temporary project to locate reference assemblies:" + Environment.NewLine + string.Join( Environment.NewLine, lines ) );

            var references = ReadReferencesFromBuildOutput( lines ).ToArray();

            File.WriteAllLines( referenceAssemlyListFile, references );

            return references;
        }

        // Buildalyzer or MSBuild.StructuredLogger could be used to get this information from binlog instead, but they are quite heavy for this task
        internal static IEnumerable<string> ReadReferencesFromBuildOutput( List<string> buildOutput )
        {
            foreach ( var line in buildOutput )
            {
                if ( !line.Contains( "csc.dll" ) )
                    continue;

                foreach ( var argument in ParseArguments( line ) )
                {
                    string referencePrefix = "/reference:";
                    if ( !argument.StartsWith( referencePrefix, StringComparison.Ordinal ) )
                        continue;

                    yield return argument.Substring( referencePrefix.Length );
                }

                yield break;
            }

            throw new InvalidOperationException( "Did not find csc command line in build output when building temporary project to locate reference assemblies" );
        }

        // https://stackoverflow.com/a/7774211/41071
        internal static IEnumerable<string> ParseArguments( string commandLine )
        {
            if ( string.IsNullOrWhiteSpace( commandLine ) )
                yield break;

            var sb = new StringBuilder();
            bool inQuote = false;
            foreach ( char c in commandLine )
            {
                if ( c == '"' && !inQuote )
                {
                    inQuote = true;
                    continue;
                }

                if ( c != '"' && !(char.IsWhiteSpace( c ) && !inQuote) )
                {
                    sb.Append( c );
                    continue;
                }

                if ( sb.Length > 0 )
                {
                    yield return sb.ToString();
                    sb.Clear();
                    inQuote = false;
                }
            }

            if ( sb.Length > 0 )
                yield return sb.ToString();
        }
    }
}
