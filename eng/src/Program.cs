// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using PostSharp.Engineering.BuildTools.Utilities;
using Spectre.Console.Cli;
using System.IO;

var product = new Product( Dependencies.Metalama )
{
    Solutions = new Solution[]
    {
        new DotNetSolution( "Metalama.sln" )
        {
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
        new DotNetSolution( "Tests\\Metalama.Framework.TestApp\\Metalama.Framework.TestApp.sln" ) { IsTestOnly = true },
        new ManyDotNetSolutions( "Tests\\Standalone\\**\\*.sln" ) { IsTestOnly = true }
    },
    PublicArtifacts = Pattern.Create(
        "Metalama.Framework.$(PackageVersion).nupkg",
        "Metalama.TestFramework.$(PackageVersion).nupkg",
        "Metalama.Framework.Redist.$(PackageVersion).nupkg",
        "Metalama.Framework.Sdk.$(PackageVersion).nupkg",
        "Metalama.Framework.Engine.$(PackageVersion).nupkg",
        "Metalama.Framework.Introspection.$(PackageVersion).nupkg",
        "Metalama.Framework.Workspaces.$(PackageVersion).nupkg",
        "Metalama.LinqPad.$(PackageVersion).nupkg" ),
    Dependencies = new[] { Dependencies.PostSharpEngineering, Dependencies.MetalamaCompiler },
    Configurations = Product.DefaultConfigurations
        .WithValue( 
            BuildConfiguration.Debug,
            Product.DefaultConfigurations.Debug with
            {
                AdditionalArtifactRules = new[]
                {
                    $@"+:%system.teamcity.build.tempDir%/Metalama/ExtractExceptions/**/*=>logs",
                    $@"+:%system.teamcity.build.tempDir%/Metalama/Extract/**/.completed=>logs",
                    $@"+:%system.teamcity.build.tempDir%/Metalama/CrashReports/**/*=>logs",
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

    var toolDirectory = Path.Combine( generatorDirectory, "bin", "Debug", "net48" );
    var toolPath = Path.Combine( toolDirectory, "Metalama.Framework.GenerateMetaSyntaxRewriter.exe" );
    var srcDirectory = arg.Context.RepoDirectory;

    if ( !ToolInvocationHelper.InvokeTool( arg.Context.Console, toolPath, srcDirectory, toolDirectory ) )
    {
        arg.IsFailed = true;
    }
}