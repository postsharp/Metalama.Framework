using System;
using System.Collections.Immutable;
using System.IO;

namespace PostSharp.Engineering.BuildTools.Build
{
    public class Product
    {
        public string ProductName { get; }

        private ImmutableArray<Solution> Solutions { get; }

        public Product( string productName, ImmutableArray<Solution> solutions )
        {
            this.ProductName = productName;
            this.Solutions = solutions;
        }

        public void Build( BuildContext context )
        {
            this.Restore( context );

            context.Console.WriteHeading( $"Building {this.ProductName}..." );

            foreach ( var solution in this.Solutions )
            {
                solution.Build( context );
            }

            context.Console.WriteSuccess( $"Building {this.ProductName} was successful." );
        }

        public void Restore( BuildContext context )
        {
            this.Prepare( context );

            context.Console.WriteHeading( $"Restoring {this.ProductName}..." );

            foreach ( var solution in this.Solutions )
            {
                solution.Restore( context );
            }

            context.Console.WriteSuccess( $"Restoring {this.ProductName} was successful." );
        }

        public void Test( BuildContext context, bool includeCoverage )
        {
            this.Build( context );

            context.Console.WriteHeading( $"Testing {this.ProductName}..." );

            foreach ( var solution in this.Solutions )
            {
                if ( solution.CanTest )
                {
                    solution.Test( context, includeCoverage );
                }
            }

            context.Console.WriteHeading( $"Testing {this.ProductName} was successful" );
        }

        public void Pack( BuildContext context, bool sign )
        {
            this.Build( context );

            context.Console.WriteHeading( $"Packing {this.ProductName}..." );

            foreach ( var solution in this.Solutions )
            {
                if ( solution.CanPack )
                {
                    solution.Pack( context );
                }
            }

            context.Console.WriteHeading( $"Packing {this.ProductName} was successful" );
        }

        public bool Prepare( BuildContext context )
        {
            var timestamp = DateTime.Now.ToString( "MMdd.HHmmss" );
            var configuration = context.Options.Configuration.ToString().ToLower();

            string packageVersion;
            string assemblyVersion;
            switch (context.Options.Version.Kind)
            {
                case VersionKind.Local:
                    {
                        // Local build with timestamp-based version and randomized package number. For the assembly version we use a local incremental file stored in the user profile.
                        var localVersionDirectory = Environment.ExpandEnvironmentVariables( "%APPDATA%\\Caravela.Engineering" );
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
                        packageVersion = "$(MainVersion)-{packageVersionSuffix}";
                        assemblyVersion = "$(MainVersion).{localVersion}";

                        context.Console.WriteImportantMessage(
                            $"PackageVersion = *-{packageVersionSuffix}, AssemblyVersion=*.{localVersion}" );
                        break;
                    }
                case VersionKind.Numbered:
                    {
                        // Build server build with a build number given by the build server
                        var versionNumber = context.Options.Version.Number;
                        packageVersion = $"$(MainVersion)-build-{configuration}.{versionNumber}";
                        assemblyVersion = $"$(MainVersion).{versionNumber}";
                        break;
                    }
                case VersionKind.Public:
                    // Public build
                    packageVersion = "$(MainVersion)`$(PackageVersionSuffix)";
                    assemblyVersion = "$(MainVersion)";
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var artifactsDir = $"`$(MSBuildThisFileDirectory)..\\artifacts\\bin\\{configuration}";

            var props = $@"
<!-- This file is generated by the engineering tooling -->
<Project>
    <Import Project=""MainVersion.props"" />
    <PropertyGroup>
        <${this.ProductName}Version>{packageVersion}</{this.ProductName}Version>
        <${this.ProductName}AssemblyVersion>{assemblyVersion}</{this.ProductName}AssemblyVersion>
    </PropertyGroup>
    <PropertyGroup>
        <!-- Adds the local output directories as nuget sources for referencing projects. -->
        <RestoreAdditionalProjectSources>`$(RestoreAdditionalProjectSources);{artifactsDir}</RestoreAdditionalProjectSources>
    </PropertyGroup>
</Project>
";
            var propsFilePath = $"eng\\{this.ProductName}Version.props";

            File.WriteAllText( propsFilePath, props );
            return true;
        }

        public void Clean( BuildContext context )
        {
            void DeleteDirectory( string directory )
            {
                if ( Directory.Exists( directory ) )
                {
                    Directory.Delete( directory, true );
                }
            }

            void CleanRecursive( string directory )
            {
                DeleteDirectory( Path.Combine( directory, "bin" ) );    
                DeleteDirectory( Path.Combine( directory, "obj" ) );

                foreach ( var subdirectory in Directory.EnumerateDirectories( directory ) )
                {
                    CleanRecursive( subdirectory );
                }
            }
            
            DeleteDirectory( Path.Combine( context.RepoDirectory, "artifacts" ) );
            CleanRecursive( context.RepoDirectory );
            
        }
    }
}