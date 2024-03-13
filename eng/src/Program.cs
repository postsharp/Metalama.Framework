﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Definitions;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using PostSharp.Engineering.BuildTools.Utilities;
using Spectre.Console.Cli;
using System;
using System.IO;
using MetalamaDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.MetalamaDependencies.V2024_1;

var product = new Product( MetalamaDependencies.Metalama )
{
    Solutions =
    [
        new DotNetSolution( "Metalama.sln" )
        {
            SolutionFilterPathForInspectCode = "Metalama.LatestRoslyn.slnf",
            SupportsTestCoverage = true,
            CanFormatCode = true,
            
            // We don't run the tests for the whole solution because they are too slow and redundant. See #34277.
            TestMethod = BuildMethod.None,
            FormatExclusions =
            [
                // Test payloads should not be formatted because it would break the test output comparison.
                // In some cases, formatting or redundant keywords may be intentional.
                "Tests\\Metalama.Framework.Tests.Integration\\Tests\\**\\*",
                "Tests\\Metalama.Framework.Tests.Integration.Internals\\Tests\\**\\*",

                // XML formatting seems to be conflicting.
                "**\\*.props", "**\\*.targets", "**\\*.csproj", "**\\*.md", "**\\*.xml", "**\\*.config"
            ]
        },
        new DotNetSolution( "Metalama.LatestRoslyn.slnf" )
        {
            SupportsTestCoverage = false,
            CanFormatCode = false,
            IsTestOnly = true
        },
        new DotNetSolution( "Tests\\Metalama.Framework.TestApp\\Metalama.Framework.TestApp.sln" )
        {
            IsTestOnly = true, TestMethod = BuildMethod.Build
        },
        new ManyDotNetSolutions( "Tests\\Standalone" )
        {
            IsTestOnly = true
        }
    ],
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
    ParametrizedDependencies =
    [
        DevelopmentDependencies.PostSharpEngineering.ToDependency(),
        MetalamaDependencies.MetalamaBackstage.ToDependency(),
        MetalamaDependencies.MetalamaCompiler.ToDependency(
            new ConfigurationSpecific<BuildConfiguration>(
                BuildConfiguration.Release, BuildConfiguration.Release, BuildConfiguration.Public
            ) ),
        MetalamaDependencies.MetalamaFrameworkRunTime.ToDependency()
    ],
    SourceDependencies = [MetalamaDependencies.MetalamaFrameworkPrivate],
    ExportedProperties = { { @"eng\Versions.props", new[] { "RoslynApiMaxVersion" } } },
    Configurations = Product.DefaultConfigurations
        .WithValue(
            BuildConfiguration.Debug,
            c => c with
            {
                AdditionalArtifactRules =
                [
                    $@"+:%system.teamcity.build.tempDir%/Metalama/ExtractExceptions/**/*=>logs",
                    $@"+:%system.teamcity.build.tempDir%/Metalama/Extract/**/.completed=>logs",
                    $@"+:%system.teamcity.build.tempDir%/Metalama/CrashReports/**/*=>logs",

                    // Do not upload uncompressed crash reports because they are too big.
                    $@"-:%system.teamcity.build.tempDir%/Metalama/CrashReports/**/*.dmp=>logs",
                ]
            } ),
    SupportedProperties = { { "PrepareStubs", "The prepare command generates stub files, instead of actual implementations." } }
};

product.PrepareCompleted += OnPrepareCompleted;
product.PrepareCompleted += TestLicensesCache.FetchOnPrepareCompleted;
product.BuildCompleted += OnBuildCompleted;

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );

static void OnPrepareCompleted( PrepareCompletedEventArgs arg )
{
    arg.Context.Console.WriteHeading( "Generating code" );

    var generatorDirectory =
        Path.Combine( arg.Context.RepoDirectory, "source-dependencies", "Metalama.Framework.Private", "src",
            "Metalama.Framework.GenerateMetaSyntaxRewriter" );
    var project =
        new DotNetSolution( Path.Combine(
            generatorDirectory,
            "Metalama.Framework.GenerateMetaSyntaxRewriter.csproj" ) );

    var settings = new BuildSettings { BuildConfiguration = BuildConfiguration.Debug };
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

    var toolDirectory = Path.Combine( generatorDirectory, "bin", "Debug", "net8.0" );
    var toolPath = Path.Combine( toolDirectory, "Metalama.Framework.GenerateMetaSyntaxRewriter.exe" );
    var srcDirectory = arg.Context.RepoDirectory;
    var commandLine = srcDirectory;

    if ( arg.Settings.Properties.ContainsKey( "PrepareStubs" ) )
    {
        commandLine = $"{commandLine} --stubs";
    }

    if ( !ToolInvocationHelper.InvokeTool( arg.Context.Console, toolPath, commandLine, toolDirectory ) )
    {
        arg.IsFailed = true;
    }
}

static void OnBuildCompleted( BuildCompletedEventArgs args )
{
    // Copy LICENSE.md to build artefacts because this file is then used in Metalama.Vsx.
    var sourceFile = Path.Combine( args.Context.RepoDirectory, "LICENSE.md" );
    var targetFile = Path.Combine( args.PrivateArtifactsDirectory, "LICENSE.md" );

    File.Copy( sourceFile, targetFile );
}
    