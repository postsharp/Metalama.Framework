using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using PostSharp.Engineering.BuildTools.Commands.Build;
using PostSharp.Engineering.BuildTools.Commands.NuGet;
using Spectre.Console;
using System;
using System.Collections.Immutable;
using System.IO;

namespace PostSharp.Engineering.BuildTools.Build
{
    public class Product
    {
        public string ProductName { get; init; } = "Unnamed";

        public ImmutableArray<Solution> Solutions { get; init; } = ImmutableArray<Solution>.Empty;

        public ImmutableArray<string> PublicArtifacts { get; init; } = ImmutableArray<string>.Empty;

        public bool Build( BuildContext context, BuildOptions options )
        {
            if ( !options.SkipDependencies &&  !this.Restore( context, options ) )
            {
                return false;
            }

            context.Console.WriteHeading( $"Building {this.ProductName}." );

            if ( !this.BuildCore( context, options ) )
            {
                return false;
            }

            context.Console.WriteSuccess( $"Building {this.ProductName} was successful." );

            // If we're doing a public build, copy public artifacts to the publish directory.
            if ( options.PublicBuild )
            {
                if ( context.Product.PublicArtifacts.IsEmpty )
                {
                    context.Console.WriteError( $"Cannot build a public version when no public artefacts have been specified." );
                    return false;
                }
                
                // Copy artifacts.
                context.Console.WriteHeading( "Copying public artifacts" );
                var matcher = new Matcher( StringComparison.OrdinalIgnoreCase );
                var artifactsDirectory = Path.Combine( context.RepoDirectory, "artifacts" );
                
                foreach ( var publicArtifacts in context.Product.PublicArtifacts )
                {
                    matcher.AddInclude( publicArtifacts );
                }

                var publicArtifactsDirectory = Path.Combine( artifactsDirectory, "publish" );
                var matches =matcher.Execute( new DirectoryInfoWrapper(  new DirectoryInfo( artifactsDirectory ) ) );
                if ( matches is { HasMatches: true } )
                {
                    foreach ( var file in matches.Files )
                    {
                        var targetFile = Path.Combine( publicArtifactsDirectory, Path.GetFileName( file.Path ) );
                            
                        context.Console.WriteMessage( file.Stem );
                        File.Copy( file.Path, targetFile );
                    }
                }

                this.BeforeSigningArtifacts( context, options );
                
                // Verify that public packages have no private dependencies.
                if ( !VerifyPublicPackageCommand.Execute( context.Console,
                    new VerifyPackageSettings { Directory = publicArtifactsDirectory } ) )
                {
                    return false;
                }
                
                // Sign public artifacts.
                var signSuccess = true;
                if ( options.Sign )
                {
                    context.Console.WriteHeading( "Signing artifacts" );
                    void Sign( string filter )
                    {
                        foreach ( var file in Directory.GetFiles( publicArtifactsDirectory, filter ) )
                        {
                            context.Console.WriteMessage( $"Signing '{file}'." );
                            
                            // TODO: Sign
                        }

                        
                    }

                    Sign( "*.nupkg" );
                    Sign( "*.vsix" );

                    if ( !signSuccess )
                    {
                        return false;
                    }
                }
                
            }
            else if ( options.Sign )
            {
                context.Console.WriteWarning( $"Cannot use --sign option in a non-public build." );
                return false;
            }

            return true;


            
        }

        protected virtual void BeforeSigningArtifacts( BuildContext context, BuildOptions options ) { }

        protected virtual bool BuildCore( BuildContext context, CommonOptions options )
        {
            foreach ( var solution in this.Solutions )
            {
                if ( !solution.Build( context, options ) )
                {
                    return false;
                }
            }

            return true;
        }

        private bool Restore( BuildContext context, CommonOptions options )
        {
            if (  !this.Prepare( context, options ) )
            {
                return false;
            }

            context.Console.WriteHeading( $"Restoring {this.ProductName}." );

            if ( !this.RestoreCore( context, options ) )
            {
                return false;
            }

            context.Console.WriteSuccess( $"Restoring {this.ProductName} was successful." );

            return true;
        }

        protected virtual bool RestoreCore( BuildContext context, CommonOptions options )
        {
            foreach ( var solution in this.Solutions )
            {
                if ( !solution.Restore( context, options ) )
                {
                    return false;
                }
            }

            return true;
        }

        public bool Test( BuildContext context, TestOptions options )
        {
            if ( !options.SkipDependencies && !this.Build( context, options ) )
            {
                return false;
            }

            context.Console.WriteHeading( $"Testing {this.ProductName}." );

            foreach ( var solution in this.Solutions )
            {
                if ( solution.CanTest )
                {
                    solution.Test( context, options );
                }
            }

            context.Console.WriteHeading( $"Testing {this.ProductName} was successful" );

            return true;
        }

        public bool Prepare( BuildContext context, CommonOptions options )
        {
            var timestamp = DateTime.Now.ToString( "MMdd.HHmmss" );
            var configuration = options.Configuration.ToString().ToLower();

            string packageVersion;
            string assemblyVersion;
            switch ( options.VersionSpec.Kind )
            {
                case VersionKind.Local:
                    {
                        // Local build with timestamp-based version and randomized package number. For the assembly version we use a local incremental file stored in the user profile.
                        var localVersionDirectory =
                            Environment.ExpandEnvironmentVariables( "%APPDATA%\\Caravela.Engineering" );
                        var localVersionFile = $"{localVersionDirectory}\\{this.ProductName}.version";
                        int localVersion;
                        if ( File.Exists( localVersionFile ) )
                        {
                            localVersion = int.Parse( File.ReadAllText( localVersionFile ) );
                        }
                        else
                        {
                            localVersion = 1;
                        }

                        if ( localVersion < 1000 ) { localVersion = 1000; }


                        if ( !Directory.Exists( localVersionDirectory ) )
                        {
                            Directory.CreateDirectory( localVersionDirectory );
                        }

                        File.WriteAllText( localVersionFile, localVersion.ToString() );


                        var packageVersionSuffix =
                            $"local-{localVersion}-{DateTime.Now.Year}{timestamp}-{new Random().Next():x8}-{configuration}";
                        packageVersion = $"$(MainVersion)-{packageVersionSuffix}";
                        assemblyVersion = $"$(MainVersion).{localVersion}";

                        context.Console.WriteImportantMessage(
                            $"PackageVersion = *-{packageVersionSuffix}, AssemblyVersion=*.{localVersion}" );

                        break;
                    }
                case VersionKind.Numbered:
                    {
                        // Build server build with a build number given by the build server
                        var versionNumber = options.VersionSpec.Number;
                        packageVersion = $"$(MainVersion)-build-{configuration}.{versionNumber}";
                        assemblyVersion = $"$(MainVersion).{versionNumber}";
                        break;
                    }
                case VersionKind.Public:
                    // Public build
                    packageVersion = "$(MainVersion)$(PackageVersionSuffix)";
                    assemblyVersion = "$(MainVersion)";
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var artifactsDir = Path.Combine( context.RepoDirectory, "artifacts", "bin", configuration );

            var props = this.GenerateVersionFile( packageVersion, assemblyVersion, artifactsDir );
            var propsFilePath = Path.Combine( context.RepoDirectory, $"eng\\{this.ProductName}Version.props" );

            context.Console.WriteMessage( $"Writing '{propsFilePath}'." );
            File.WriteAllText( propsFilePath, props );
            return true;
        }

        protected virtual string GenerateVersionFile( string packageVersion, string assemblyVersion,
            string? artifactsDir )
        {
            var props = $@"
<!-- This file is generated by the engineering tooling -->
<Project>
    <Import Project=""MainVersion.props"" />
    <PropertyGroup>
        <{this.ProductName}Version>{packageVersion}</{this.ProductName}Version>
        <{this.ProductName}AssemblyVersion>{assemblyVersion}</{this.ProductName}AssemblyVersion>
    </PropertyGroup>
    <PropertyGroup>
        <!-- Adds the local output directories as nuget sources for referencing projects. -->
        <RestoreAdditionalProjectSources>`$(RestoreAdditionalProjectSources);{artifactsDir}</RestoreAdditionalProjectSources>
    </PropertyGroup>
</Project>
";
            return props;
        }

        public void Clean( BuildContext context )
        {
            void DeleteDirectory( string directory )
            {
                if ( Directory.Exists( directory ) )
                {
                    context.Console.WriteMessage( $"Deleting directory '{directory}'." );
                    Directory.Delete( directory, true );
                }
            }

            void CleanRecursive( string directory )
            {
                DeleteDirectory( Path.Combine( directory, "bin" ) );
                DeleteDirectory( Path.Combine( directory, "obj" ) );

                foreach ( var subdirectory in Directory.EnumerateDirectories( directory ) )
                {
                    if ( subdirectory == Path.Combine( context.RepoDirectory, "eng" ) )
                    {
                        // Skip the engineering directory.
                        continue;
                    }

                    CleanRecursive( subdirectory );
                }
            }

            context.Console.WriteHeading( $"Cleaning {this.ProductName}." );
            DeleteDirectory( Path.Combine( context.RepoDirectory, "artifacts" ) );
            CleanRecursive( context.RepoDirectory );
        }

      
    }
}