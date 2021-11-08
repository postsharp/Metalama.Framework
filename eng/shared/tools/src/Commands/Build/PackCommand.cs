using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Console;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class PackCommand : BaseBuildCommand
    {
        public PackCommand( Product product ) : base( product, "pack", "Pack all relevant projects" )
        {
            this.AddOption( new Option<bool>( "--sign", "Signs the assemblies and packages" ) );
            
            this.Handler =
                CommandHandler.Create<InvocationContext, BuildConfiguration, int, bool, Verbosity, bool>( this.Execute );

        }

        public int Execute( InvocationContext context, BuildConfiguration configuration, int number, bool @public,
            Verbosity verbosity, bool sign )
        {
            var buildOptions = new BuildOptions( VersionSpec.Create( number, @public ), configuration, verbosity );
            if ( !BuildContext.TryCreate( context, buildOptions, out var buildContext ) )
            {
                return 1;
            }

            this.Product.Pack( buildContext, sign );
            return 0;
        }
    }
}