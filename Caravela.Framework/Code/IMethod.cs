using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a method.
    /// </summary>
    public interface IMethod : IMember
    {
        /// <remarks>
        /// Gets an object representing the method return type and custom attributes, or  <c>null</c> for methods that don't have return types: constructors and finalizers.
        /// </remarks>
        IParameter? ReturnParameter { get; }

        /// <summary>
        /// Gets the method return type.
        /// </summary>
        IType ReturnType { get; }

        /// <summary>
        /// Gets the list of local functions declared by the current method.
        /// </summary>
        IReadOnlyList<IMethod> LocalFunctions { get; }

        /// <summary>
        /// Gets the list of parameters of the current method.
        /// </summary>
        IReadOnlyList<IParameter> Parameters { get; }

        /// <summary>
        /// Gets the generic parameters of the current method.
        /// </summary>
        IReadOnlyList<IGenericParameter> GenericParameters { get; }

        /// <summary>
        /// Gets the kind of method (such as <see cref="Code.MethodKind.Default"/> or <see cref="Code.MethodKind.PropertyGet"/>.
        /// </summary>
        MethodKind MethodKind { get; }

        // dynamic Invoke(params object[] args);
    }

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

    public readonly struct OptionalValue
    {
        public bool HasValue { get;  }
        public object? Value { get; }

        public OptionalValue( object? value ) : this()
        {
            this.Value = value;
            this.HasValue = true;
        }
    }

}