using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using PostSharp.Engineering.BuildTools.Commands.Coverage;
using PostSharp.Engineering.BuildTools.Commands.NuGet;
using PostSharp.Engineering.BuildTools.Utilities;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class Product
    {
        public string ProductName { get; init; } = "Unnamed";

        public ImmutableArray<Solution> Solutions { get; init; } = ImmutableArray<Solution>.Empty;

        public ImmutableArray<string> PublicArtifacts { get; init; } = ImmutableArray<string>.Empty;

        public bool Build( BuildContext context, BuildOptions options )
        {
            if ( !options.NoDependencies && !this.Prepare( context, options ) )
            {
                return false;
            }


            if ( !this.BuildCore( context, options ) )
            {
                return false;
            }

            // If we're doing a public build, copy public artifacts to the publish directory.
            if ( options.PublicBuild )
            {
                if ( context.Product.PublicArtifacts.IsEmpty )
                {
                    context.Console.WriteError(
                        $"Cannot build a public version when no public artefacts have been specified." );
                    return false;
                }

                // We have to read the version from the file we have generated - using MSBuild, because it contains properties.
                var packageVersion = this.ReadPackageVersion(context);


                // Copy artifacts.
                context.Console.WriteHeading( "Copying public artifacts" );
                var matcher = new Matcher( StringComparison.OrdinalIgnoreCase );
                var artifactsDirectory = Path.Combine( context.RepoDirectory, "artifacts" );

                foreach ( var publicArtifacts in context.Product.PublicArtifacts )
                {
                    matcher.AddInclude( publicArtifacts
                        .Replace( "$(PackageVersion)", packageVersion )
                        .Replace( "$(Configuration)", options.Configuration.ToString() ) );
                }

                var publicArtifactsDirectory = Path.Combine( artifactsDirectory, "publish" );

                if ( !Directory.Exists( publicArtifactsDirectory ) )
                {
                    Directory.CreateDirectory( publicArtifactsDirectory );
                }

                var matches = matcher.Execute( new DirectoryInfoWrapper( new DirectoryInfo( artifactsDirectory ) ) );
                if ( matches is { HasMatches: true } )
                {
                    foreach ( var file in matches.Files )
                    {
                        var targetFile = Path.Combine( publicArtifactsDirectory, Path.GetFileName( file.Path ) );

                        context.Console.WriteMessage( file.Path );
                        File.Copy( Path.Combine( artifactsDirectory, file.Path ), targetFile, true );
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

                    // Restore signing tools.
                    var restoreTool = Path.Combine( context.RepoDirectory, "eng", "shared", "tools", "Restore.ps1" );
                    var signTool = Path.Combine( context.RepoDirectory, "tools", "SignClient.exe" );
                    var signToolConfig = Path.Combine( context.RepoDirectory, "eng", "shared", "tools", "signclient-appsettings.json" );
                    var signToolSecret = Environment.GetEnvironmentVariable( "SIGNSERVER_SECRET" );

                    if ( signToolSecret == null )
                    {
                        context.Console.WriteError( "The SIGNSERVER_SECRET environment variable is not defined." );
                        return false;
                    }
                    
                    if ( !ToolInvocationHelper.InvokePowershell( context.Console, restoreTool, "SignClient",
                        context.RepoDirectory ) )
                    {
                        return false;
                    }

                    void Sign( string filter )
                    {
                        if ( Directory.EnumerateFiles( publicArtifactsDirectory, filter ).Any() )
                        {
                            // We don't pass the secret so it does not get printed. We pass an environment variable reference instead.
                            // The ToolInvocationHelper will expand it.

                            signSuccess = signSuccess && ToolInvocationHelper.InvokeTool(
                                context.Console,
                                signTool,
                                $"Sign --baseDirectory {publicArtifactsDirectory} --input {filter} --config {signToolConfig} --name {this.ProductName} --user sign-caravela@postsharp.net --secret %SIGNSERVER_SECRET%",
                                context.RepoDirectory );
                        }
                    }

                    Sign( "*.nupkg" );
                    Sign( "*.vsix" );

                    if ( !signSuccess )
                    {
                        return false;
                    }
                    
                    context.Console.WriteSuccess( "Signing artifacts was successful." );
                }
            }
            else if ( options.Sign )
            {
                context.Console.WriteWarning( $"Cannot use --sign option in a non-public build." );
                return false;
            }

            context.Console.WriteSuccess( $"Building the whole {this.ProductName} product was successful." );

            return true;
        }

        private string? ReadPackageVersion(BuildContext context)
        {
            var versionFilePath = context.VersionFilePath;
            var versionFile = Project.FromFile(versionFilePath, new ProjectOptions());
            var packageVersion = versionFile.Properties.Single(p => p.Name == this.ProductName + "Version")
                .EvaluatedValue;
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
            return packageVersion;
        }

        protected virtual void BeforeSigningArtifacts( BuildContext context, BuildOptions options ) { }

        protected virtual bool BuildCore( BuildContext context, BuildOptions options )
        {
            foreach ( var solution in this.Solutions )
            {
                if ( options.IncludeTests || !solution.IsTestOnly )
                {
                    context.Console.WriteHeading( $"Building {solution.Name}." );

                    if ( !solution.Restore( context, options ) )
                    {
                        return false;
                    }

                    if ( solution.IsTestOnly )
                    {
                        // Never try to pack solutions.
                        if ( !solution.Build( context, options ) )
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if ( !solution.Pack( context, options ) )
                        {
                            return false;
                        }
                    }

                    context.Console.WriteSuccess( $"Building {solution.Name} was successful." );
                }
            }

            return true;
        }


        public bool Test( BuildContext context, TestOptions options )
        {
            if ( !options.NoDependencies && !this.Build( context, (BuildOptions)options.WithIncludeTests( true ) ) )
            {
                return false;
            }

            ImmutableDictionary<string, string> properties;
            var testResultsDir = Path.Combine( context.RepoDirectory, "TestResults" );

            if ( options.AnalyzeCoverage )
            {
                // Removing the TestResults directory so that we reset the code coverage information.
                if ( Directory.Exists( testResultsDir ) ) {
                    Directory.Delete( testResultsDir, true );
                }
                properties = options.AnalyzeCoverage
                    ? ImmutableDictionary.Create<string,string>()
                        .Add( "CollectCoverage", "True")
                        .Add( "CoverletOutput", testResultsDir + "\\" )
                    : ImmutableDictionary<string, string>.Empty;
            }
            else
            {
                properties = ImmutableDictionary<string, string>.Empty;
            }


           

            foreach ( var solution in this.Solutions )
            {
                var solutionOptions = options;
                
                if ( options.AnalyzeCoverage && solution.SupportsTestCoverage )
                {
                    solutionOptions = (TestOptions)options.WithAdditionalProperties( properties ).WithoutConcurrency();
                }
                
                context.Console.WriteHeading( $"Testing {solution.Name}." );
                solution.Test( context, solutionOptions );
                context.Console.WriteSuccess( $"Testing {solution.Name} was successful" );
            }

            if ( options.AnalyzeCoverage )
            {
                if ( !AnalyzeCoverageCommand.Execute( context.Console,
                    new AnalyzeCoverageSettings { Path = Path.Combine( testResultsDir, "coverage.net5.0.json" ) } ) )
                {
                    return false;
                }
            }

            context.Console.WriteSuccess( $"Testing {this.ProductName} was successful" );

            return true;
        }

        public bool Prepare( BuildContext context, CommonOptions options )
        {
            context.Console.WriteHeading( "Preparing the version file" );
            
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
            
            context.Console.WriteSuccess(
                $"Preparing the version file was successful. {this.ProductName}Version={this.ReadPackageVersion( context )}" );

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
        <RestoreAdditionalProjectSources>$(RestoreAdditionalProjectSources);{artifactsDir}</RestoreAdditionalProjectSources>
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