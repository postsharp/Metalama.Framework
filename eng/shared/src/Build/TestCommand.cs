namespace PostSharp.Engineering.BuildTools.Build
{
    public class TestCommand : BaseCommand<TestOptions>
    {
        protected override bool ExecuteCore( BuildContext buildContext, TestOptions options )
        {
            return buildContext.Product.Test( buildContext, options );
        }
    }
}