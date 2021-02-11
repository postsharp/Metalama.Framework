namespace Caravela.Framework.Code
{
    public interface IMethodBuilder : IMethod, IMemberBuilder
    {
        IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, OptionalValue optionalValue = default );
        IGenericParameterBuilder AddGenericParameter( string name );
        
        /// <remarks>
        /// Gets an object allowing to read and modify the method return type and custom attributes,
        /// or  <c>null</c> for methods that don't have return types: constructors and finalizers.
        /// </remarks>
        new IParameterBuilder? ReturnParameter { get; }
        
        /// <summary>
        /// Gets or sets the method return type.
        /// </summary>
        new IType? ReturnType { get; set; }

    }
}