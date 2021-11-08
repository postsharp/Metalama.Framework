using PostSharp.Engineering.BuildTools.Build;
using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class ProductCommand : Command
    {
        public ProductCommand( Product product ) : base( "product",
            $"Build, test or pack the whole product '{product.ProductName}'" )
        {
            this.AddCommand( new BuildCommand( product ) );
            this.AddCommand( new PrepareCommand( product ) );
            this.AddCommand( new TestCommand( product ) );
        }
    }
}