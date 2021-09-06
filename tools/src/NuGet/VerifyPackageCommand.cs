// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using NuGet.Versioning;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PostSharp.Engineering.BuildTools.Nuget
{
    internal class VerifyPackageCommand : Command
    {
        private static readonly Dictionary<string, bool> _cache = new();

        public VerifyPackageCommand() : base( "verify", "Verifies all packages in a directory." )
        {
            this.AddOption(
                new Option( "-d", "Directory containing the packages" )
                {
                    Name = "directory", IsRequired = true, Argument = new Argument<DirectoryInfo>()
                } );

            this.Handler = CommandHandler.Create<InvocationContext, DirectoryInfo>( Execute );
        }

        private static int Execute( InvocationContext context, DirectoryInfo directory )
        {
            var success = true;

            var files = Directory.GetFiles( directory.FullName, "*.nupkg" );

            if ( files.Length == 0 )
            {
                context.Console.Error.Write( $"No matching package found in '{directory.FullName}'." );
                return 1;
            }

            foreach ( var file in files )
            {
                success &= VerifyPackage( context.Console, directory.FullName, file );
            }

            return success ? 0 : 2;
        }

        private static bool VerifyPackage( IConsole console, string directory, string inputPath )
        {
            var inputShortPath = Path.GetFileName( inputPath );

            var success = true;

            using var archive = ZipFile.Open( inputPath, ZipArchiveMode.Read );

            var nuspecEntry = archive.Entries.Single( entry => entry.FullName.EndsWith( ".nuspec" ) );

            if ( nuspecEntry == null )
            {
                console.Error.WriteLine( $"{inputPath} Cannot find the nuspec file." );
                return false;
            }

            XDocument nuspecXml;
            XmlReader xmlReader;
            using ( var nuspecStream = nuspecEntry.Open() )
            {
                xmlReader = new XmlTextReader( nuspecStream );
                nuspecXml = XDocument.Load( xmlReader );
            }

            var ns = nuspecXml.Root.Name.Namespace.NamespaceName;

            var namespaceManager = new XmlNamespaceManager( xmlReader.NameTable );
            namespaceManager.AddNamespace( "p", ns );

            var httpClient = new HttpClient();

            foreach ( var dependency in nuspecXml.XPathSelectElements( "//p:dependency", namespaceManager ) )
            {
                // Get dependency id and version.
                var dependentId = dependency.Attribute( "id" ).Value;
                var versionRangeString = dependency.Attribute( "version" ).Value;
                if ( !VersionRange.TryParse( versionRangeString, out var versionRange ) )
                {
                    console.Error.WriteLine(
                        $"{inputShortPath}: cannot parse the version range '{versionRangeString}'." );
                    success = false;
                    continue;
                }

                // Check if it's present in the directory.
                var localFile = Path.Combine( directory,
                    dependentId + "." + versionRange.MinVersion.ToNormalizedString() + ".nupkg" );

                if ( !File.Exists( localFile ) )
                {
                    // Check if the dependency is present on nuget.org.
                    var uri =
                        $"https://www.nuget.org/packages/{dependentId}/{versionRange.MinVersion.ToNormalizedString()}";

                    if ( !_cache.TryGetValue( uri, out var packageFound ) )
                    {
                        var httpResult = httpClient.SendAsync( new HttpRequestMessage( HttpMethod.Get, uri ) ).Result;
                        packageFound = httpResult.IsSuccessStatusCode;
                        _cache.Add( uri, packageFound );
                    }

                    if ( !packageFound )
                    {
                        console.Error.WriteLine(
                            $"{inputShortPath}: {dependentId} {versionRangeString} is not public." );
                        success = false;
                        continue;
                    }
                }
            }

            if ( success )
            {
                console.Out.WriteLine( inputShortPath + ": correct" );
            }

            return success;
        }
    }
}