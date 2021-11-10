namespace PostSharp.Engineering.BuildTools.Commands.Build
{
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