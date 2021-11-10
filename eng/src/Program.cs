// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Commands.Build;
using Spectre.Console.Cli;
using System.Collections.Immutable;

namespace Build
{
    internal class Program
    {
        private static int Main( string[] args )
        {
            var product = new Product
            {
                ProductName = "Caravela",
                Solutions = ImmutableArray.Create<Solution>(
                    new DotNetSolution( "Caravela.sln" ) { SupportsTestCoverage = true },
                    new DotNetSolution( "Tests\\Caravela.Framework.TestApp\\Caravela.Framework.TestApp.sln" )
                    {
                        IsTestOnly = true
                    } ),
                PublicArtifacts = ImmutableArray.Create(
                    "bin\\$(Configuration)\\Caravela.Framework.$(PackageVersion).nupkg",
                    "bin\\$(Configuration)\\Caravela.TestFramework.$(PackageVersion).nupkg",
                    "bin\\$(Configuration)\\Caravela.Framework.Redist.$(PackageVersion).nupkg",
                    "bin\\$(Configuration)\\Caravela.Framework.Sdk.$(PackageVersion).nupkg",
                    "bin\\$(Configuration)\\Caravela.Framework.Impl.$(PackageVersion).nupkg",
                    "bin\\$(Configuration)\\Caravela.Framework.DesignTime.Contracts.$(PackageVersion).nupkg" )
            };
            var commandApp = new CommandApp();
            commandApp.AddProductCommands( product );

            return commandApp.Run( args );
        }
    }
}