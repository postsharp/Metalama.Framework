// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Project;
using System;
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
        /// Gets information about the project from which the compilation was created.
        /// </summary>
        IProject Project { get; }

        /// <summary>
        /// Gets the assembly name, without version and public key.
        /// </summary>
        string AssemblyName { get; }

        /// <summary>
        /// Gets the list of types declared in the current compilation, in all namespaces, but not the nested types.
        /// </summary>
        INamedTypeList Types { get; }

        /// <summary>
        /// Gets a service that allows to create type instances and compare them.
        /// </summary>
        ITypeFactory TypeFactory { get; }

        /// <summary>
        /// Gets the list of managed resources in the current compilation.
        /// </summary>
        [Obsolete( "Not implemented." )]
        IReadOnlyList<IManagedResource> ManagedResources { get; }

        /// <summary>
        /// Gets a service allowing to compare types and declarations considers equal two instances that represent
        /// the same type or declaration even if they belong to different compilation versions.
        /// </summary>
        IDeclarationComparer InvariantComparer { get; }

        /// <summary>
        /// Gets the global namespace (i.e. the one with an empty name).
        /// </summary>
        INamespace GlobalNamespace { get; }

        /// <summary>
        /// Gets a namespace given its full name.
        /// </summary>
        INamespace? GetNamespace( string ns );

        /// <summary>
        /// Gets the aspects of a given type on a given declaration.
        /// </summary>
        /// <param name="declaration">The declaration on which the aspects are requested.</param>
        /// <typeparam name="T">The type of aspects.</typeparam>
        /// <returns>The collection of aspects of type <typeparamref name="T"/> on <paramref name="declaration"/>.</returns>
        IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration )
            where T : IAspect;

        /// <summary>
        /// Gets the set of types, in the current compilation, that are derived from a given base type (given as an <see cref="INamedType"/>).
        /// </summary>
        /// <param name="baseType">The base type.</param>
        /// <param name="deep">When <c>true</c>, all derived types and their descendants are recursively returned. When <c>false</c>,
        /// only the first level of types in the current compilation is returned.</param>
        IEnumerable<INamedType> GetDerivedTypes( INamedType baseType, bool deep = true );

        /// <summary>
        /// Gets the set of types, in the current compilation, that are derived from a given base type (given as a <see cref="Type"/>).
        /// </summary>
        /// <param name="baseType">The base type.</param>
        /// <param name="deep">When <c>true</c>, all derived types and their descendants are recursively returned. When <c>false</c>,
        /// only the first level of types in the current compilation is returned.</param>
        IEnumerable<INamedType> GetDerivedTypes( Type baseType, bool deep = true );

        /// <summary>
        /// Gets the version of the current compilation in the Caravela pipeline. This number is only informational.
        /// </summary>
        int Revision { get; }
    }
}