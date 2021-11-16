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
using System.IO.Compression;
using System.Linq;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class Product
    {
        public string ProductName { get; init; } = "Unnamed";

        public ImmutableArray<Solution> Solutions { get; init; } = ImmutableArray<Solution>.Empty;


        public ImmutableArray<PublishingTarget> PublishingTargets { get; init; } =
            ImmutableArray<PublishingTarget>.Empty;


        public bool Build( BuildContext context, BuildOptions options )
        {
            // Validate options.
            if ( options.PublicBuild )
            {
                if ( options.BuildConfiguration != BuildConfiguration.Release )
                {
                    context.Console.WriteError(
                        $"Cannot build a public version of a {options.BuildConfiguration} build without --force." );
                    return false;
                }
            }


            // Build dependencies.
            if ( !options.NoDependencies && !this.Prepare( context, options ) )
            {
                return false;
            }

            // We have to read the version from the file we have generated - using MSBuild, because it contains properties.
            var packageVersion = this.ReadVersionFile( context );



            var artifactsDirectory = Path.Combine( context.RepoDirectory, "artifacts" );
            var privateArtifactsDir = Path.Combine( artifactsDirectory, "private" );

            

            // Build.
            if ( !this.BuildCore( context, options ) )
            {
                return false;
            }

            // Zipping internal artifacts.
            void CreateZip( string directory )
            {
                if ( options.CreateZip )
                {
                    var zipFile = Path.Combine( directory, $"{this.ProductName}-{packageVersion}.zip" );

                    context.Console.WriteMessage( $"Creating '{zipFile}'." );
                    var tempFile = Path.Combine( Path.GetTempPath(), Guid.NewGuid() + ".zip" );
                    ZipFile.CreateFromDirectory( directory,
                        tempFile,
                        CompressionLevel.Optimal, false );
                    File.Move( tempFile, zipFile );
                }
            }

            CreateZip( privateArtifactsDir );

            // If we're doing a public build, copy public artifacts to the publish directory.
            if ( options.PublicBuild )
            {
                // Copy artifacts.
                context.Console.WriteHeading( "Copying public artifacts" );
                var matcher = new Matcher( StringComparison.OrdinalIgnoreCase );

                foreach ( var publishingTarget in this.PublishingTargets )
                {
                    if ( publishingTarget.SupportsPublicPublishing )
                    {
                        publishingTarget.Artifacts.AddToMatcher( matcher, packageVersion.PackageVersion, options.BuildConfiguration.ToString() );
                    }
                }


                var publicArtifactsDirectory = Path.Combine( artifactsDirectory, "public" );

                if ( !Directory.Exists( publicArtifactsDirectory ) )
                {
                    Directory.CreateDirectory( publicArtifactsDirectory );
                }


                var matches = matcher.Execute( new DirectoryInfoWrapper( new DirectoryInfo( privateArtifactsDir ) ) );
                if ( matches is { HasMatches: true } )
                {
                    foreach ( var file in matches.Files )
                    {
                        var targetFile = Path.Combine( publicArtifactsDirectory, Path.GetFileName( file.Path ) );

                        context.Console.WriteMessage( file.Path );
                        File.Copy( Path.Combine( privateArtifactsDir, file.Path ), targetFile, true );
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
                    var signToolConfig = Path.Combine( context.RepoDirectory, "eng", "shared", "tools",
                        "signclient-appsettings.json" );
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

                    // Zipping public artifacts.
                    CreateZip( publicArtifactsDirectory );

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

        private (string PackageVersion, string Configuration) ReadVersionFile( BuildContext context )
        {
            var versionFilePath = context.VersionFilePath;
            var versionFile = Project.FromFile( versionFilePath, new ProjectOptions() );
            var packageVersion = versionFile
                .Properties
                .Single( p => p.Name == this.ProductName + "Version" )
                .EvaluatedValue;
            if ( string.IsNullOrEmpty( packageVersion ) )
            {
                throw new InvalidOperationException( "PackageVersion should not be null." );
            }
            var configuration = versionFile
                .Properties
                .Single( p => p.Name == this.ProductName + "BuildConfiguration" )
                .EvaluatedValue;
            if ( string.IsNullOrEmpty( configuration ) )
            {
                throw new InvalidOperationException( "BuildConfiguration should not be null." );
            }
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
            return (packageVersion, configuration);
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
            if ( !options.NoDependencies && !this.Build( context, (BuildOptions) options.WithIncludeTests( true ) ) )
            {
                return false;
            }

            ImmutableDictionary<string, string> properties;
            var testResultsDir = Path.Combine( context.RepoDirectory, "TestResults" );

            if ( options.AnalyzeCoverage )
            {
                // Removing the TestResults directory so that we reset the code coverage information.
                if ( Directory.Exists( testResultsDir ) )
                {
                    Directory.Delete( testResultsDir, true );
                }

                properties = options.AnalyzeCoverage
                    ? ImmutableDictionary.Create<string, string>()
                        .Add( "CollectCoverage", "True" )
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
                    solutionOptions = (TestOptions) options.WithAdditionalProperties( properties ).WithoutConcurrency();
                }

                context.Console.WriteHeading( $"Testing {solution.Name}." );
                if ( !solution.Test( context, solutionOptions ) )
                {
                    return false;
                }

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
            if ( !options.NoDependencies )
            {
                this.Clean( context );
            }


            context.Console.WriteHeading( "Preparing the version file" );

            var configuration = options.BuildConfiguration.ToString().ToLower();

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
                            localVersion = int.Parse( File.ReadAllText( localVersionFile ) ) + 1;
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
                            $"local-{Environment.UserName}-{configuration}.{localVersion}";
                        packageVersion = $"$(MainVersion)-{packageVersionSuffix}";
                        assemblyVersion = $"$(MainVersion).{localVersion}";


                        break;
                    }
                case VersionKind.Numbered:
                    {
                        // Build server build with a build number given by the build server
                        var versionNumber = options.VersionSpec.Number;
                        packageVersion = $"$(MainVersion).{versionNumber}-dev-{configuration}";
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

            var artifactsDir = Path.Combine( context.RepoDirectory, "artifacts", "private" );

            var props = this.GenerateVersionFile( packageVersion, assemblyVersion, configuration, artifactsDir );
            var propsFilePath = Path.Combine( context.RepoDirectory, $"eng\\{this.ProductName}Version.props" );

            context.Console.WriteMessage( $"Writing '{propsFilePath}'." );
            File.WriteAllText( propsFilePath, props );

            context.Console.WriteSuccess(
                $"Preparing the version file was successful. {this.ProductName}Version={this.ReadVersionFile( context ).PackageVersion}" );

            return true;
        }

        protected virtual string GenerateVersionFile( string packageVersion, string assemblyVersion, string configuration, string? artifactsDir )
        {
            var props = $@"
<!-- This file is generated by the engineering tooling -->
<Project>
    <Import Project=""MainVersion.props"" />
    <PropertyGroup>
        <{this.ProductName}Version>{packageVersion}</{this.ProductName}Version>
        <{this.ProductName}AssemblyVersion>{assemblyVersion}</{this.ProductName}AssemblyVersion>
        <{this.ProductName}BuildConfiguration>{configuration}</{this.ProductName}BuildConfiguration>
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

        public bool Publish( BuildContext context, PublishOptions options )
        {
            context.Console.WriteHeading( "Publishing files" );

            if ( !this.PublishDirectory( context, options,
                Path.Combine( context.RepoDirectory, "artifacts", "private" ),
                false ) )
            {
                return false;
            }

            if ( options.Public )
            {
                if ( !this.PublishDirectory( context, options,
                    Path.Combine( context.RepoDirectory, "artifacts", "public" ),
                    true ) )
                {
                    return false;
                }
            }

            context.Console.WriteSuccess( "Publishing has succeeded." );

            return true;
        }

        private bool PublishDirectory( BuildContext context, PublishOptions options, string directory, bool isPublic )
        {
            var success = true;

            var versionFile =  this.ReadVersionFile( context );
                
            foreach ( var publishingTarget in this.PublishingTargets )
            {
                var matcher = new Matcher( StringComparison.OrdinalIgnoreCase );

                if ( (publishingTarget.SupportsPrivatePublishing && !isPublic) ||
                     (publishingTarget.SupportsPublicPublishing && isPublic) )
                {
                    publishingTarget.Artifacts.AddToMatcher( matcher, versionFile.PackageVersion, versionFile.Configuration );
                }

                var matchingResult =
                    matcher.Execute( new DirectoryInfoWrapper( new DirectoryInfo( directory ) ) );


                foreach ( var file in matchingResult.Files )
                {
                    if ( Path.GetExtension( file.Path ).Equals( publishingTarget.MainExtension ) )
                    {
                        if ( file.Path.Contains( "-local-" ) )
                        {
                            context.Console.WriteError( "Cannot publish a local build." );
                            return false;
                        }

                        switch ( publishingTarget.Execute( context, options,
                            Path.Combine( directory, file.Path ),
                            isPublic ) )
                        {
                            case SuccessCode.Success:
                                break;

                            case SuccessCode.Error:
                                success = false;
                                break;

                            case SuccessCode.Fatal:
                                return false;
                        }
                    }
                }
            }
        

            if ( !success )
            {
                context.Console.WriteError( "Publishing has failed." );
            }


            return success;
        }
    }
}