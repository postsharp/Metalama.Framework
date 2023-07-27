// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Definitions;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using PostSharp.Engineering.BuildTools.Utilities;
using Spectre.Console.Cli;
using System.IO;
using MetalamaDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.MetalamaDependencies.V2023_1;

var product = new Product( MetalamaDependencies.Metalama )
{
    Solutions = new Solution[]
    {
        new DotNetSolution( "Metalama.sln" )
        {
            SolutionFilterPathForInspectCode = "Metalama.LatestRoslyn.slnf",
            SupportsTestCoverage = true,
            CanFormatCode = true,
            FormatExclusions = new[]
            {
                // Test payloads should not be formatted because it would break the test output comparison.
                // In some cases, formatting or redundant keywords may be intentional.
                "Tests\\Metalama.Framework.Tests.Integration\\Tests\\**\\*",
                "Tests\\Metalama.Framework.Tests.Integration.Internals\\Tests\\**\\*",
                
                // XML formatting seems to be conflicting.
                "**\\*.props",
                "**\\*.targets",
                "**\\*.csproj",
                "**\\*.md",
                "**\\*.xml",
                "**\\*.config"
            }
        },
        new DotNetSolution( "Tests\\Metalama.Framework.TestApp\\Metalama.Framework.TestApp.sln" ) { IsTestOnly = true, TestMethod = BuildMethod.Build },
        new ManyDotNetSolutions( "Tests\\Standalone" ),
    },
    PublicArtifacts = Pattern.Create(
        "Metalama.Framework.$(PackageVersion).nupkg",
        "Metalama.Testing.UnitTesting.$(PackageVersion).nupkg",
        "Metalama.Testing.AspectTesting.$(PackageVersion).nupkg",
        "Metalama.Framework.Redist.$(PackageVersion).nupkg",
        "Metalama.Framework.Sdk.$(PackageVersion).nupkg",
        "Metalama.Framework.Engine.$(PackageVersion).nupkg",
        "Metalama.Framework.CompileTimeContracts.$(PackageVersion).nupkg",
        "Metalama.Framework.Introspection.$(PackageVersion).nupkg",
        "Metalama.Framework.Workspaces.$(PackageVersion).nupkg",
        "Metalama.Tool.$(PackageVersion).nupkg" ),
    ParametrizedDependencies= new[] { DevelopmentDependencies.PostSharpEngineering.ToDependency(), MetalamaDependencies.MetalamaCompiler.ToDependency( new ConfigurationSpecific<BuildConfiguration>( 
        BuildConfiguration.Release, BuildConfiguration.Release, BuildConfiguration.Public
        )) },    
    Configurations = Product.DefaultConfigurations
        .WithValue( 
            BuildConfiguration.Debug,
            c => c with
            {
                AdditionalArtifactRules = new[]
                {
                    $@"+:%system.teamcity.build.tempDir%/Metalama/ExtractExceptions/**/*=>logs",
                    $@"+:%system.teamcity.build.tempDir%/Metalama/Extract/**/.completed=>logs",
                    $@"+:%system.teamcity.build.tempDir%/Metalama/CrashReports/**/*=>logs",

                    // Do not upload uncompressed crash reports because they are too big.
                    $@"-:%system.teamcity.build.tempDir%/Metalama/CrashReports/**/*.dmp=>logs",
                }
            } )
};

        

product.PrepareCompleted += OnPrepareCompleted;

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );

static void OnPrepareCompleted( PrepareCompletedEventArgs arg )
{
    arg.Context.Console.WriteHeading( "Generating code" );

    var generatorDirectory =
        Path.Combine( arg.Context.RepoDirectory, "Build", "Metalama.Framework.GenerateMetaSyntaxRewriter" );
    var project =
        new DotNetSolution( Path.Combine(
            generatorDirectory,
            "Metalama.Framework.GenerateMetaSyntaxRewriter.csproj" ) );

    var settings = new BuildSettings() { BuildConfiguration = BuildConfiguration.Debug };
    if ( !project.Restore( arg.Context, settings ) )
    {
        arg.IsFailed = true;
        return;
    }

    if ( !project.Build( arg.Context, settings ) )
    {
        arg.IsFailed = true;
        return;
    }

    var toolDirectory = Path.Combine( generatorDirectory, "bin", "Debug", "net60" );
    var toolPath = Path.Combine( toolDirectory, "Metalama.Framework.GenerateMetaSyntaxRewriter.exe" );
    var srcDirectory = arg.Context.RepoDirectory;

    if ( !ToolInvocationHelper.InvokeTool( arg.Context.Console, toolPath, srcDirectory, toolDirectory ) )
    {
        arg.IsFailed = true;
    }
}