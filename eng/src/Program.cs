// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using Spectre.Console.Cli;
using System.Collections.Immutable;

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
        Dependencies.CaravelaCompiler )
};

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );