using PostSharp.Engineering.BuildTools.Build;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class BuildOptions : CommonOptions
    {
        [Description( "Signs the assemblies and packages" )]
        [CommandOption( "--sign" )]
        public bool Sign { get; }
    }
    public class BuildCommand : BaseProductCommand<BuildOptions>
    {
        protected override int ExecuteCore( BuildContext buildContext, BuildOptions options )
        {
            if ( !buildContext.Product.Build( buildContext, options ) )
            {
                return 2;
            }


            return 0;
        }
    }
}