namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows to add members to an attribute created by <see cref="ICodeElementBuilder.AddAttribute"/>.
    /// </summary>
    public interface IAttributeBuilder : IAttribute
    {
        /// <summary>
        /// Adds a new named argument to the attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddNamedArgument( string name, object? value );
    }
}