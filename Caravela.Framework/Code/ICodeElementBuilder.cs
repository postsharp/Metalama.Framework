namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows to complete the construction of a code element that has been created by an advice.
    /// </summary>
    public interface ICodeElementBuilder : ICodeElement
    {
        bool IsReadOnly { get; }

        /// <summary>
        /// Adds a custom attribute to the current code element.
        /// </summary>
        /// <param name="type">Type of the custom attribute.</param>
        /// <param name="constructorArguments">Arguments of the constructors.</param>
        /// <returns></returns>
        IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments );

        /// <summary>
        /// Removes all custom attributes of a given type from the current code element.
        /// </summary>
        /// <param name="type">TYpe of custom attributes to be removed.</param>
        void RemoveAttributes( INamedType type );
    }
}