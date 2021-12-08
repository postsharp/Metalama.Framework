// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using PostSharp.Engineering.BuildTools.Utilities;
using Spectre.Console.Cli;
using System;
using System.Collections.Immutable;
using System.IO;

var product = new Product
{
    ProductName = "Caravela",
    Solutions = ImmutableArray.Create<Solution>(
        new DotNetSolution( "Caravela.sln" )
        {
            SupportsTestCoverage = true,
            CanFormatCode = true,
            FormatExclusions = ImmutableArray.Create(
                "Tests\\Caravela.Framework.Tests.Integration\\Tests\\**\\*",
                "Tests\\Caravela.Framework.Tests.Integration.Internals\\Tests\\**\\*" )
        },
        new DotNetSolution( "Tests\\Caravela.Framework.TestApp\\Caravela.Framework.TestApp.sln" )
        {
            IsTestOnly = true
        } ),
    PublicArtifacts = Pattern.Create(
        "Caravela.Framework.$(PackageVersion).nupkg",
        "Caravela.TestFramework.$(PackageVersion).nupkg",
        "Caravela.Framework.Redist.$(PackageVersion).nupkg",
        "Caravela.Framework.Sdk.$(PackageVersion).nupkg",
        "Caravela.Framework.Impl.$(PackageVersion).nupkg",
        "Caravela.Framework.DesignTime.Contracts.$(PackageVersion).nupkg" ),
    Dependencies = ImmutableArray.Create(
        Dependencies.PostSharpEngineering,
        Dependencies.CaravelaCompiler,
        Dependencies.PostSharpBackstageSettings )
};

product.PrepareCompleted += OnPrepareCompleted;

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );

static bool OnPrepareCompleted( (BuildContext Context, BaseBuildSettings Settings) arg )
{
    arg.Context.Console.WriteHeading( "Generating code" );

    var generatorDirectory = Path.Combine( arg.Context.RepoDirectory, "Build", "Caravela.Framework.GenerateMetaSyntaxRewriter" );
    var project = new DotNetSolution( Path.Combine( generatorDirectory, "Caravela.Framework.GenerateMetaSyntaxRewriter.csproj" ) );

    if ( !project.Restore( arg.Context, new BuildSettings() ) )
    {
        return false;
    }

    if ( !project.Build( arg.Context, new BuildSettings() ) )
    {
        return false;
    }

    var toolDirectory = Path.Combine( generatorDirectory, "bin", "Debug", "net48" );
    var toolPath = Path.Combine( toolDirectory, "Caravela.Framework.GenerateMetaSyntaxRewriter.exe" );
    if ( !ToolInvocationHelper.InvokeTool( arg.Context.Console, toolPath, "", toolDirectory ) )
    {
        return false;
    }

    var targetFile = Path.Combine( arg.Context.RepoDirectory, "Caravela.Framework.Impl", "Templating", "MetaSyntaxRewriter.g.cs" );
    if ( File.Exists( targetFile ) )
    {
        File.Delete( targetFile );
    }

    File.Copy( Path.Combine( toolDirectory, "MetaSyntaxRewriter.g.cs" ), targetFile );

    return true;
}