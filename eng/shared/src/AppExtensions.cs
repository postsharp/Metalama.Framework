// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Csproj;
using PostSharp.Engineering.BuildTools.Dependencies;
using PostSharp.Engineering.BuildTools.Engineering;
using PostSharp.Engineering.BuildTools.NuGet;
using Spectre.Console.Cli;
using System.Linq;

namespace PostSharp.Engineering.BuildTools
{
    public static class AppExtensions
    {
        public static void AddProductCommands( this CommandApp app, Product? product = null )
        {
            if ( product != null )
            {
                app.Configure(
                    root =>
                    {
                        root.Settings.StrictParsing = true;

                        root.AddCommand<PrepareCommand>( "prepare" )
                            .WithData( product )
                            .WithDescription( "Creates the files that are required to build the product" );

                        root.AddCommand<BuildCommand>( "build" )
                            .WithData( product )
                            .WithDescription( "Builds all packages in the product (implies 'prepare')" );

                        root.AddCommand<TestCommand>( "test" )
                            .WithData( product )
                            .WithDescription( "Builds all packages then run all tests (implies 'build')" );

                        root.AddCommand<PublishCommand>( "publish" )
                            .WithData( product )
                            .WithDescription( "Publishes all packages that have been previously built by the 'build' command" );

                        if ( product.Solutions.Any( s => s.CanFormatCode ) )
                        {
                            root.AddCommand<FormatCommand>( "format" ).WithData( product ).WithDescription( "Formats the code" );
                        }

                        root.AddBranch(
                            "dependencies",
                            dependencies =>
                            {
                                dependencies.AddCommand<ListDependenciesCommand>( "list" )
                                    .WithData( product )
                                    .WithDescription( "Lists the dependencies of this product" );

                                dependencies.AddCommand<GenerateDependencyFileCommand>( "local" )
                                    .WithData( product )
                                    .WithDescription( "Generates the Dependencies.props to consume local repos." );

                                dependencies.AddCommand<PrintDependenciesCommand>( "print" )
                                    .WithData( product )
                                    .WithDescription( "Prints the dependency file." );
                            } );

                        root.AddBranch(
                            "engineering",
                            engineering =>
                            {
                                engineering.AddCommand<PushEngineeringCommand>( "push" )
                                    .WithData( product )
                                    .WithDescription(
                                        $"Copies the changes in {product.EngineeringDirectory}/shared to the local engineering repo, but does not commit nor push." );

                                engineering.AddCommand<PullEngineeringCommand>( "pull" )
                                    .WithData( product )
                                    .WithDescription(
                                        $"Copies the remote engineering repo to {product.EngineeringDirectory}/shared. Automatically pulls 'master'." );
                            } );
                        root.AddBranch(
                            "tools",
                            tools =>
                            {
                                tools.AddBranch(
                       "csproj",
                       csproj => csproj.AddCommand<AddProjectReferenceCommand>( "add-project-reference" )
                           .WithDescription( "Adds a <ProjectReference> item to *.csproj in a directory" ) );

                                tools.AddBranch(
                                    "nuget",
                                    nuget =>
                                    {
                                        nuget.AddCommand<RenamePackagesCommand>( "rename" )
                                            .WithDescription( "Renames all packages in a directory" );

                                        nuget.AddCommand<VerifyPublicPackageCommand>( "verify-public" )
                                            .WithDescription( "Verifies that all packages in a directory have only references to packages published on nuget.org." );
                                    } );
                            } );
                    } );
            }
        }
    }
}