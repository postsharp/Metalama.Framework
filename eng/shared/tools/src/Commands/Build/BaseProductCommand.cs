using PostSharp.Engineering.BuildTools.Build;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{

    public abstract class BaseProductCommand<T> : Command<T>
        where T : CommonOptions
    {
        public sealed override int Execute( CommandContext context, T settings )
        {
            
            if ( !BuildContext.TryCreate( context, out var buildContext ) )
            {
                return 1;
            }
            else
            {
                buildContext.Console.Out.Write( new FigletText( buildContext.Product.ProductName )
                    .LeftAligned()
                    .Color( Color.Purple ) );
                
                return this.ExecuteCore( buildContext, settings );
            }
        }

        protected abstract int ExecuteCore( BuildContext context, T options );
    }
}