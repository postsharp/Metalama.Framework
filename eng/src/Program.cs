// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using PostSharp.Engineering.BuildTools.Utilities;
using Spectre.Console.Cli;
using System.IO;

var product = new Product
{
    ProductName = "Metalama",
    Solutions = new[]
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

                            // This file should not be formatted because it contains assembly aliases, and JetBrains tools
                            // don't support them properly.
                            "Metalama.Framework.Engine\\Utilities\\SymbolId.cs"
                        }
                    },
                    new DotNetSolution( "Tests\\Metalama.Framework.TestApp\\Metalama.Framework.TestApp.sln" )
                    {
                        IsTestOnly = true
                    }
    },
    PublicArtifacts = Pattern.Create(
        "Metalama.Framework.$(PackageVersion).nupkg",
        "Metalama.TestFramework.$(PackageVersion).nupkg",
        "Metalama.Framework.Redist.$(PackageVersion).nupkg",
        "Metalama.Framework.Sdk.$(PackageVersion).nupkg",
        "Metalama.Framework.Engine.$(PackageVersion).nupkg",
        "Metalama.Framework.DesignTime.Contracts.$(PackageVersion).nupkg" ),
    Dependencies = new[]
    {
        Dependencies.PostSharpEngineering,
        Dependencies.MetalamaCompiler
    }
};

product.PrepareCompleted += OnPrepareCompleted;

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );

static bool OnPrepareCompleted( (BuildContext Context, BaseBuildSettings Settings) arg )
{
    arg.Context.Console.WriteHeading( "Generating code" );

    var generatorDirectory = Path.Combine( arg.Context.RepoDirectory, "Build", "Metalama.Framework.GenerateMetaSyntaxRewriter" );
    var project = new DotNetSolution( Path.Combine( generatorDirectory, "Metalama.Framework.GenerateMetaSyntaxRewriter.csproj" ) );

    if ( !project.Restore( arg.Context, new BuildSettings() ) )
    {
        return false;
    }

    if ( !project.Build( arg.Context, new BuildSettings() ) )
    {
        return false;
    }

    var toolDirectory = Path.Combine( generatorDirectory, "bin", "Debug", "net48" );
    var toolPath = Path.Combine( toolDirectory, "Metalama.Framework.GenerateMetaSyntaxRewriter.exe" );
    if ( !ToolInvocationHelper.InvokeTool( arg.Context.Console, toolPath, "", toolDirectory ) )
    {
        return false;
    }

    var targetFile = Path.Combine( arg.Context.RepoDirectory, "Metalama.Framework.Engine", "Templating", "MetaSyntaxRewriter.g.cs" );
    if ( File.Exists( targetFile ) )
    {
        File.Delete( targetFile );
    }

    File.Copy( Path.Combine( toolDirectory, "MetaSyntaxRewriter.g.cs" ), targetFile );

    return true;
}