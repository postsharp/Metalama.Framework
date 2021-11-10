namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class CleanCommand : BaseProductCommand<CommonOptions>
    {
        protected override int ExecuteCore( BuildContext buildContext, CommonOptions options )
        {
            buildContext.Product.Clean( buildContext );
            return 0;
        }
    }
}