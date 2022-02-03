// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a set of projects. An <see cref="IProjectSet"/> can include several instances of the <see cref="Project"/>
    /// for the same file if they target multiple frameworks, one <see cref="Project"/> instance per framework. You
    /// can create a subset with the <see cref="GetSubset"/> method.
    /// </summary>
    public interface IProjectSet : ICompilationSet
    {
        /// <summary>
        /// Gets the projects in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<Project> Projects { get; }

        /// <summary>
        /// Returns a subset of the current subset.
        /// </summary>
        /// <param name="filter">A predicate that determines if a project must be a part of the new subset.</param>
        /// <returns></returns>
        IProjectSet GetSubset( Predicate<Project> filter );

        /// <summary>
        /// Gets all diagnostics reported in the <i>source code</i> loaded in the current subset.  
        /// </summary>
        ImmutableArray<IIntrospectionDiagnostic> SourceDiagnostics { get; }

        /// <summary>
        /// Gets the result of the compilation of the project by Metalama.
        /// </summary>
        IMetalamaCompilationSet MetalamaOutput { get; }

        /// <summary>
        /// Gets a declaration in the current subset. 
        /// </summary>
        /// <param name="projectPath">Path of the project.</param>
        /// <param name="targetFramework">Target framework, or an empty string.</param>
        /// <param name="declarationId">Serialized identifier of the declaration obtained  with <see cref="IRef{T}.ToSerializableId"/>.</param>
        /// <param name="metalamaOutput"></param>
        /// <returns></returns>
        IDeclaration? GetDeclaration( string projectPath, string targetFramework, string declarationId, bool metalamaOutput );
    }
}