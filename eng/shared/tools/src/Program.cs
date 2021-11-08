using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Commands.Build;
using PostSharp.Engineering.BuildTools.Commands.Coverage;
using PostSharp.Engineering.BuildTools.Commands.Csproj;
using PostSharp.Engineering.BuildTools.Commands.NuGet;
using System.Collections.Immutable;
using System.CommandLine;

namespace PostSharp.Engineering.BuildTools
{
    internal class Program
    {
        private static int Main( string[] args )
        {
            var rootCommand = new RootCommand { };

            rootCommand.AddCommonCommands( new Product( "Test", ImmutableArray<Solution>.Empty ) );

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync( args ).Result;
        }
    }

    public static class RootCommandExtensions
    {
        public static void AddCommonCommands( this RootCommand rootCommand, Product? product = null )
        {
            rootCommand.AddCommand( new CsprojCommand() );
            rootCommand.AddCommand( new NuGetCommand() );
            rootCommand.AddCommand( new CoverageCommand() );
            if ( product != null )
            {
                rootCommand.AddCommand( new ProductCommand( product ) );
            }
        }
    }
}