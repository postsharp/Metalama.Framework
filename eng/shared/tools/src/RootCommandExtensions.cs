using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Commands.Build;
using PostSharp.Engineering.BuildTools.Commands.Csproj;
using PostSharp.Engineering.BuildTools.Commands.NuGet;
using Spectre.Console.Cli;

namespace PostSharp.Engineering.BuildTools
{
    public static class RootCommandExtensions
    {
        public static void AddCommonCommands( this IConfigurator configurator, Product? product = null )
        {
            configurator.AddBranch( "csproj",
                x => x.AddCommand<AddProjectReferenceCommand>( "add-project-reference" )
                    .WithDescription( "Adds a <ProjectReference> item to *.csproj in a directory" ) );
            configurator.AddBranch( "nuget", x =>
            {
                x.AddCommand<RenamePackagesCommand>( "rename" ).WithDescription( "Renames all packages in a directory" );
                x.AddCommand<VerifyPublicPackageCommand>( "verify-public" ).WithDescription(
                    "Verifies that all packages in a directory have only references to packages published on nuget.org." );
            } );

            if ( product != null )
            {
                configurator.AddBranch( "product",
                    p =>
                    {
                        p.AddCommand<PrepareCommand>( "prepare" ).WithData( product ).WithDescription( "Creates the files that are required to build the product" );
                        p.AddCommand<BuildCommand>( "build" ).WithData( product ).WithDescription( "Builds all packages in the product. Implies 'prepare'." );
                        p.AddCommand<TestCommand>( "test" ).WithData( product ).WithDescription( "Builds all packages then run all tests. Implies 'build'." );
                        p.AddCommand<TestCommand>( "build-artifacts" ).WithData( product ).WithDescription( "Prepare all artefacts and copy them in the 'artifacts' directory. Implies 'test'." );
                    } );
            }
        }
    }
}