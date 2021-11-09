using PostSharp.Engineering.BuildTools.Build;
using Spectre.Console.Cli;
using System.Collections.Immutable;

namespace PostSharp.Engineering.BuildTools
{
    internal class Program
    {
        private static int Main( string[] args )
        {
            var testProduct = new Product { ProductName = "Test", Solutions = ImmutableArray<Solution>.Empty };
            var commandApp = new CommandApp();
            commandApp.Configure( c => c.AddCommonCommands( testProduct ) );

            return commandApp.Run( args );
        }
    }
}