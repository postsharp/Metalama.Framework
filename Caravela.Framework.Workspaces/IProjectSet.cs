// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Workspaces
{
    /// <summary>
    /// Represents a set of projects. An <see cref="IProjectSet"/> can include several instances of the <see cref="Project"/>
    /// for the same file if they target multiple frameworks, one <see cref="Project"/> instance per framework. You
    /// can create a subset with the <see cref="GetSubset"/> method.
    /// </summary>
    public interface IProjectSet
    {
        /// <summary>
        /// Gets the projects in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<Project> Projects { get; }

        /// <summary>
        /// Gets all target frameworks of projects in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<TargetFramework> TargetFrameworks { get; }

        /// <summary>
        /// Gets all types in the current <see cref="ProjectSet"/>, including nested types.
        /// </summary>
        ImmutableArray<INamedType> Types { get; }

        /// <summary>
        /// Gets all methods in the current <see cref="ProjectSet"/>, except local methods.
        /// </summary>
        ImmutableArray<IMethod> Methods { get; }

        /// <summary>
        /// Gets all fields in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<IField> Fields { get; }

        /// <summary>
        /// Gets all properties in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<IProperty> Properties { get; }

        /// <summary>
        /// Gets all properties and properties in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<IFieldOrProperty> FieldsAndProperties { get; }

        /// <summary>
        /// Gets all constructors in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<IConstructor> Constructors { get; }

        /// <summary>
        /// Gets all events in the current <see cref="ProjectSet"/>.
        /// </summary>
        ImmutableArray<IEvent> Events { get; }

        /// <summary>
        /// Gets all diagnostics reported in the projects in the current <see cref="ProjectSet"/>. Note that the diagnostics reported
        /// by Caravela are not yet included in this set.
        /// </summary>
        ImmutableArray<IDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Returns a subset of the current subset.
        /// </summary>
        /// <param name="filter">A predicate that determines if a project must be a part of the new subset.</param>
        /// <returns></returns>
        IProjectSet GetSubset( Predicate<Project> filter );

        /// <summary>
        /// Gets a declaration in the current subset. 
        /// </summary>
        /// <param name="projectPath">Path of the project.</param>
        /// <param name="targetFramework">Target framework, or an empty string.</param>
        /// <param name="declarationId">Serialized identifier of the declaration obtained  with <see cref="IRef{T}.ToSerializableId"/>.</param>
        /// <returns></returns>
        IDeclaration? GetDeclaration( string projectPath, string targetFramework, string declarationId );
    }
}