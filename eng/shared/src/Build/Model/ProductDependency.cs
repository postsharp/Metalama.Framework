namespace PostSharp.Engineering.BuildTools.Build.Model
{
    public class ProductDependency
    {
        public string Name { get; }

        public ProductDependency( string name )
        {
            this.Name = name;
        }
    }
}
