using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Reactive;

// TODO: InternalImplement
namespace Caravela.Framework.Code
{
    [CompileTime]
    public interface ICompilation : ICodeElement
    {
        IReactiveCollection<INamedType> DeclaredTypes { get; }

        IReactiveCollection<INamedType> DeclaredAndReferencedTypes { get; }

        IReactiveGroupBy<string?, INamedType> DeclaredTypesByNamespace { get; }

        // TODO: do assembly and module attributes need to be differentiated?

        /// <summary>
        /// Get type based on its full name, as used in reflection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For nested types, this means using <c>+</c>, e.g. to get <see cref="System.Environment.SpecialFolder"/>, use <c>System.Environment+SpecialFolder</c>.
        /// </para>
        /// <para>
        /// For generic type definitions, this required using <c>`</c>, e.g. to get <see cref="List{T}"/>, use <c>System.Collections.Generic.List`1</c>.
        /// </para>
        /// <para>
        /// Constructed generic types (e.g. <c>List&lt;int&gt;</c>) are not supported, for those, use <see cref="INamedType.MakeGenericType"/>.
        /// </para>
        /// </remarks>
        INamedType? GetTypeByReflectionName(string reflectionName);

        IType? GetTypeByReflectionType( Type type );
    }

    // TODO: IArrayType etc.
}
