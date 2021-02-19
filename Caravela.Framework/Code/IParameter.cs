namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a parameter of a method or property.
    /// </summary>
    public interface IParameter : ICodeElement
    {
        /// <summary>
        /// Gets the <c>in</c>, <c>out</c>, <c>ref</c> parameter type modifier.
        /// </summary>
        RefKind RefKind { get; }

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        IType ParameterType { get; }

        /// <summary>
        /// Gets the parameter type, or <c>null</c> for <see cref="IMethod.ReturnParameter"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the parameter position, or <c>-1</c> for <see cref="IMethod.ReturnParameter"/>.
        /// </summary>
        int Index { get; }

        /// <remarks>
        /// Gets the default value of the parameter, or  <c>default</c> if the parameter type is a struct and the default
        /// value of the parameter is the default value of the struct type.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The parameter has no default value.</exception>
        OptionalValue DefaultValue { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsParams { get; }

        /// <summary>
        /// Gets the parent <see cref="IMethod"/>, <see cref="IConstructor"/> or <see cref="IProperty"/>.
        /// </summary>
        IMember DeclaringMember { get; }
    }
}