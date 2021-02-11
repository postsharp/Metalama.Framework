namespace Caravela.Framework.Code
{
    public interface IAttributeBuilder : IAttribute
    {
        void AddNamedArgument( string name, object? value );

    }
}