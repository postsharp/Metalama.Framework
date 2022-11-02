﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a set of types compiled together. See also <see cref="IProject"/>.
    /// </summary>
    [CompileTime]
    public interface ICompilation : IAssembly
    {
        /// <summary>
        /// Gets information about the project from which the compilation was created.
        /// </summary>
        IProject Project { get; }

        /// <summary>
        /// Gets the list of managed resources in the current compilation.
        /// </summary>
        [Obsolete( "Not implemented." )]
        IReadOnlyList<IManagedResource> ManagedResources { get; }

        /// <summary>
        /// Gets a equality comparers that can be used with declarations of this compilation.
        /// </summary>
        ICompilationComparers Comparers { get; }

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
        /// Gets the version of the current compilation in the Metalama pipeline. This number is only informational.
        /// </summary>
        int Revision { get; }

        IDeclaration GetDeclarationFromId( DeclarationSerializableId declarationId );

        /// <summary>
        /// Gets a value indicating whether the current compilation is partial, i.e. incomplete. Metalama uses partial compilations
        /// at design time, when only the closure of modified types are being incrementally recompiled.
        /// </summary>
        bool IsPartial { get; }
    }
}