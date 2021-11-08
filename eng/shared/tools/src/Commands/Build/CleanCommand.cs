using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Console;
using System.CommandLine.Invocation;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class CleanCommand : BaseBuildCommand
    {

        public CleanCommand( Product product ) : base( product, "clean", "Cleans all solutions" )
        {
            this.Handler =
                CommandHandler.Create<InvocationContext, BuildConfiguration, int, bool, Verbosity>( this.Execute );

        }

        public int Execute( InvocationContext context, BuildConfiguration configuration, int number, bool @public,
            Verbosity verbosity )
        {
            var buildOptions = new BuildOptions( VersionSpec.Create( number, @public ), configuration, verbosity );
            if ( !BuildContext.TryCreate( context, buildOptions, out var buildContext ) )
            {
                return 1;
            }

            this.Product.Clean( buildContext );
            return 0;
        }
    }
}