using PostSharp.Engineering.BuildTools.Build;

namespace PostSharp.Engineering.BuildTools.Dependencies
{
    public class ListDependenciesCommand : BaseCommand<BaseCommandSettings>
    {
        protected override bool ExecuteCore( BuildContext context, BaseCommandSettings options )
        {
            var productDependencies = context.Product.Dependencies;

            if ( productDependencies.IsDefaultOrEmpty )
            {
                context.Console.WriteImportantMessage( $"{context.Product.ProductName} has no dependency." );
            }
            else
            {

                context.Console.WriteImportantMessage( $"{context.Product.ProductName} has {productDependencies.Length} dependencies:" );
                
                for ( var i = 0; i < productDependencies.Length; i++ )
                {
                    context.Console.WriteImportantMessage( $"    {i + 1}: {productDependencies[i].Name}" );
                }
            }

            return true;
        }
    }
}