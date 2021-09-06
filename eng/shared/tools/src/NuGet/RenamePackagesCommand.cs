// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PostSharp.Engineering.BuildTools.Nuget
{
    internal class RenamePackagesCommand : Command
    {
        public RenamePackagesCommand() : base( "rename", "Rename NuGet packages in a directory" )
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

            var files = Directory.GetFiles( directory.FullName, "Microsoft.*.nupkg" );

            if ( files.Length == 0 )
            {
                context.Console.Error.WriteLine( $"No matching package found in '{directory.FullName}'." );
            }

            foreach ( var file in files )
            {
                success &= RenamePackage( context.Console, directory.FullName, file );
            }

            return success ? 0 : 2;
        }

        private static bool RenamePackage( IConsole console, string directory, string inputPath )
        {
            console.Out.WriteLine( "Processing " + inputPath );

            var outputPath = Path.Combine(
                Path.GetDirectoryName( inputPath )!,
                Path.GetFileName( inputPath ).Replace( "Microsoft", "Caravela.Roslyn" ) );

            File.Copy( inputPath, outputPath, true );

            using var archive = ZipFile.Open( outputPath, ZipArchiveMode.Update );

            var oldNuspecEntry = archive.Entries.Single( entry => entry.FullName.EndsWith( ".nuspec" ) );

            if ( oldNuspecEntry == null )
            {
                console.Error.WriteLine( "Usage: Cannot find the nuspec file." );
                return false;
            }

            XDocument nuspecXml;
            XmlReader xmlReader;
            using ( var nuspecStream = oldNuspecEntry.Open() )
            {
                xmlReader = new XmlTextReader( nuspecStream );
                nuspecXml = XDocument.Load( xmlReader );
            }

            var ns = nuspecXml.Root.Name.Namespace.NamespaceName;

            // Rename the packageId.
            var packageIdElement =
                nuspecXml.Root.Element( XName.Get( "metadata", ns ) ).Element( XName.Get( "id", ns ) );
            var oldPackageId = packageIdElement.Value;
            var newPackageId = oldPackageId.Replace( "Microsoft", "Caravela.Roslyn" );
            var packageVersion = nuspecXml.Root.Element( XName.Get( "metadata", ns ) )
                .Element( XName.Get( "version", ns ) ).Value;
            packageIdElement.Value = newPackageId;

            // Rename the dependencies.
            var namespaceManager = new XmlNamespaceManager( xmlReader.NameTable );
            namespaceManager.AddNamespace( "p", ns );

            foreach ( var dependency in nuspecXml.XPathSelectElements( "//p:dependency", namespaceManager ) )
            {
                var dependentId = dependency.Attribute( "id" ).Value;

                if ( dependentId.StartsWith( "Microsoft" ) )
                {
                    var dependencyPath = Path.Combine( directory, dependentId + "." + packageVersion + ".nupkg" );

                    if ( File.Exists( dependencyPath ) )
                    {
                        dependency.Attribute( "id" ).Value = dependentId.Replace( "Microsoft", "Caravela.Roslyn" );
                    }
                    else
                    {
                        // The dependency is not produced by this repo.
                    }
                }
            }

            oldNuspecEntry.Delete();
            var newNuspecEntry = archive.CreateEntry( newPackageId + ".nuspec", CompressionLevel.Optimal );

            using ( var outputStream = newNuspecEntry.Open() )
            {
                nuspecXml.Save( outputStream );
            }

            return true;
        }
    }
}