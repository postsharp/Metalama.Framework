// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Collections;
using System.Collections.Generic;

// TODO: InternalImplement
namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a set of types compiled together. See also <see cref="IProject"/>.
    /// </summary>
    [CompileTimeOnly]
    public interface ICompilation : IAssembly
    {
        /// <summary>
        /// Gets the list of types declared in the current compilation, in all namespaces, but not the nested types.
        /// </summary>
        INamedTypeList DeclaredTypes { get; }

        /// <summary>
        /// Gets a service that allows to create type instances and compare them.
        /// </summary>
        ITypeFactory TypeFactory { get; }

        /// <summary>
        /// Gets the list of managed resources in the current compilation.
        /// </summary>
        IReadOnlyList<IManagedResource> ManagedResources { get; }

        /// <summary>
        /// Gets a service allowing to compare types and declarations considers equal two instances that represent
        /// the same type or declaration even if they belong to different compilation versions.
        /// </summary>
        IDeclarationComparer InvariantComparer { get; }

        IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration )
            where T : IAspect;
    }

    internal interface ICompilationInternal : ICompilation
    {
        ICompilationHelpers Helpers { get; }
    }

    internal interface ICompilationHelpers
    {
        IteratorInfo GetIteratorInfo( IMethod method );
    }
}