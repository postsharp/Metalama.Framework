namespace PostSharp.Engineering.BuildTools.Build
{
    public class PrepareCommand : BaseCommand<BaseBuildSettings>
    {
        protected override bool ExecuteCore( BuildContext buildContext, BaseBuildSettings options )
        {
            return buildContext.Product.Prepare( buildContext, options );
        }
    }
}