namespace PostSharp.Engineering.BuildTools.Build
{
    public class CleanCommand : BaseCommand<BaseBuildSettings>
    {
        protected override bool ExecuteCore( BuildContext buildContext, BaseBuildSettings options )
        {
            buildContext.Product.Clean( buildContext, options );
            return true;
        }
    }
}