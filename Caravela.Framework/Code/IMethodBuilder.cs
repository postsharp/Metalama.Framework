namespace Caravela.Framework.Code
{
    /// <summary>
    /// Allows to complete the construction of a method that has been created by an advice.
    /// </summary>
    public interface IMethodBuilder : IMethod, IMemberBuilder
    {
        /// <summary>
        /// Appends a parameter to the method.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="type">Parameter type.</param>
        /// <param name="refKind"><c>out</c>, <c>ref</c>...</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>A <see cref="IParameterBuilder"/> that allows you to further build the new parameter.</returns>
        IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, OptionalValue defaultValue = default );

        /// <summary>
        /// Adds a generic parameter to the method.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A <see cref="IParameterBuilder"/> that allows you to further build the new parameter.</returns>
        IGenericParameterBuilder AddGenericParameter( string name );

        /// <remarks>
        /// Gets an object allowing to read and modify the method return type and custom attributes,
        /// or  <c>null</c> for methods that don't have return types: constructors and finalizers.
        /// </remarks>
        new IParameterBuilder ReturnParameter { get; }

        /// <summary>
        /// Gets or sets the method return type.
        /// </summary>
        new IType ReturnType { get; set; }
    }
}