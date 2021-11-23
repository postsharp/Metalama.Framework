namespace PostSharp.Engineering.BuildTools.Build
{
    public class BuildCommand : BaseCommand<BuildOptions>
    {
        protected override bool ExecuteCore( BuildContext buildContext, BuildOptions options )
        {
            return buildContext.Product.Build( buildContext, options );
        }
    }
}