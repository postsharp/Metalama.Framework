using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Console;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class TestCommand : BaseBuildCommand
    {
        public TestCommand( Product product ) : base( product, "test", "Execute all required tests" )
        {
            this.AddOption( new Option<bool>( "--coverage", "Compute test coverage and emit warnings for gaps" ) );
            
            this.Handler =
                CommandHandler.Create<InvocationContext, BuildConfiguration, int, bool, Verbosity, bool>( this.Execute );

        }

        public int Execute( InvocationContext context, BuildConfiguration configuration, int number, bool @public,
            Verbosity verbosity, bool coverage )
        {
            var buildOptions = new BuildOptions( VersionSpec.Create( number, @public ), configuration, verbosity );
            if ( !BuildContext.TryCreate( context, buildOptions, out var buildContext ) )
            {
                return 1;
            }

            this.Product.Test( buildContext, coverage );
            return 0;
        }
    }
}