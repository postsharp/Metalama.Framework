// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build.Model;
using Spectre.Console.Cli;
using System.Collections.Immutable;

namespace BuildMetalama
{
    internal static class Program
    {
        private static int Main( string[] args )
        {
            var privateSource = new NugetSource( "%INTERNAL_NUGET_PUSH_URL%", "%INTERNAL_NUGET_API_KEY%" );
            var publicSource = new NugetSource( "https://api.nuget.org/v3/index.json", "%NUGET_ORG_API_KEY%" );

            // These packages are published to internal and private feeds.
            var publicPackages = new ParametricString[]
            {
                "Metalama.Framework.$(PackageVersion).nupkg", "Metalama.TestFramework.$(PackageVersion).nupkg",
                "Metalama.Framework.Redist.$(PackageVersion).nupkg",
                "Metalama.Framework.Sdk.$(PackageVersion).nupkg", "Metalama.Framework.Impl.$(PackageVersion).nupkg",
                "Metalama.Framework.DesignTime.Contracts.$(PackageVersion).nupkg"
            };

            var publicPublishing = new NugetPublishTarget(
                Pattern.Empty.Add( publicPackages ),
                privateSource,
                publicSource );

            var product = new Product
            {
                ProductName = "Metalama",
                Solutions = ImmutableArray.Create<Solution>(
                    new DotNetSolution( "Metalama.sln" )
                    {
                        SupportsTestCoverage = true,
                        CanFormatCode = true,
                        FormatExclusions = ImmutableArray.Create(
                            "Tests\\Metalama.Framework.Tests.Integration\\Tests\\**\\*",
                            "Tests\\Metalama.Framework.Tests.Integration.Internals\\Tests\\**\\*" )
                    },
                    new DotNetSolution( "Tests\\Metalama.Framework.TestApp\\Metalama.Framework.TestApp.sln" )
                    {
                        IsTestOnly = true
                    } ),
                PublishingTargets = ImmutableArray.Create<PublishingTarget>( publicPublishing ),
                Dependencies = ImmutableArray.Create(
                    new ProductDependency( "Metalama.Compiler" ),
                    new ProductDependency( "PostSharp.Engineering" ),
                    new ProductDependency( "PostSharp.Backstage.Settings" ) )
            };

            var commandApp = new CommandApp();
            commandApp.AddProductCommands( product );

            return commandApp.Run( args );
        }
    }
}