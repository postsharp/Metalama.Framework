﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build.Model;
using Spectre.Console.Cli;
using System.Collections.Immutable;

namespace Build
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
                "Caravela.Framework.$(PackageVersion).nupkg",
                "Caravela.TestFramework.$(PackageVersion).nupkg",
                "Caravela.Framework.Redist.$(PackageVersion).nupkg",
                "Caravela.Framework.Sdk.$(PackageVersion).nupkg",
                "Caravela.Framework.Impl.$(PackageVersion).nupkg",
                "Caravela.Framework.DesignTime.Contracts.$(PackageVersion).nupkg"
            };

            var publicPublishing = new NugetPublishTarget(
                Pattern.Empty.Add( publicPackages ),
                privateSource,
                publicSource );

            // These packages are published to private feeds only.
            var privatePublishing = new NugetPublishTarget(
                Pattern.Empty.Add( "*.nupkg" ).Remove( publicPackages ),
                privateSource );

            var product = new Product
            {
                ProductName = "Caravela",
                Solutions = ImmutableArray.Create<Solution>(
                    new DotNetSolution( "Caravela.sln" ) { SupportsTestCoverage = true, CanFormatCode = true },
                    new DotNetSolution( "Tests\\Caravela.Framework.TestApp\\Caravela.Framework.TestApp.sln" ) { IsTestOnly = true } ),
                PublishingTargets = ImmutableArray.Create<PublishingTarget>( publicPublishing, privatePublishing )
            };

            var commandApp = new CommandApp();
            commandApp.AddProductCommands( product );

            return commandApp.Run( args );
        }
    }
}