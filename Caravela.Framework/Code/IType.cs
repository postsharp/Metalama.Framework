using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a constructed type, for instance an array, a generic type instance, a pointer.
    /// A class, struct, enum or delegate are represented as an <see cref="INamedType"/>, which
    /// derive from <see cref="IType"/>.
    /// </summary>
    [CompileTime]
    public interface IType : IDisplayable
    {
        /// <summary>
        /// Gets the kind of type.
        /// </summary>
        TypeKind TypeKind { get; }

        /// <summary>
        /// Gets the service used to compare this type or construct other types.
        /// This member is used by <see cref="TypeExtensions"/> and is not meant to be used directly in user code.
        /// </summary>
        ITypeFactory TypeFactory { get; }

    }
}