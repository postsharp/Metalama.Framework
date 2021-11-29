// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Workspaces
{
    /// <summary>
    /// Represents a set of projects. A <see cref="ProjectSet"/> can include several instances of the <see cref="Project"/>
    /// for the same file if they target multiple frameworks, one <see cref="Project"/> instance per framework. You
    /// can create a subset with the <see cref="GetSubset"/> method.
    /// </summary>
    public sealed class ProjectSet
    {
        private readonly ConcurrentDictionary<string, ProjectSet> _subsets = new();

        /// <summary>
        /// Gets the projects in the current <see cref="ProjectSet"/>.
        /// </summary>
        public ImmutableArray<Project> Projects { get; }

        internal ProjectSet( ImmutableArray<Project> projects )
        {
            // This gets a snapshot of the collection of project at the moment the object is created.
            this.Projects = projects;
        }

        /// <summary>
        /// Gets all target frameworks of projects in the current <see cref="ProjectSet"/>.
        /// </summary>
        [Memo]
        public ImmutableArray<TargetFramework> TargetFrameworks
            => this.Projects.Select( p => p.TargetFramework )
                .WhereNotNull()
                .Distinct()
                .OrderBy( s => s )
                .Select( s => new TargetFramework( s ) )
                .ToImmutableArray();

        /// <summary>
        /// Gets all types in the current <see cref="ProjectSet"/>, including nested types.
        /// </summary>
        [Memo]
        public ImmutableArray<INamedType> Types => this.Projects.SelectMany( p => p.Types ).ToImmutableArray();

        /// <summary>
        /// Gets all methods in the current <see cref="ProjectSet"/>, except local methods.
        /// </summary>
        [Memo]
        public ImmutableArray<IMethod> Methods => this.Types.SelectMany( t => t.Methods ).ToImmutableArray();

        /// <summary>
        /// Gets all fields in the current <see cref="ProjectSet"/>.
        /// </summary>
        [Memo]
        public ImmutableArray<IField> Fields => this.Types.SelectMany( t => t.Fields ).ToImmutableArray();

        /// <summary>
        /// Gets all properties in the current <see cref="ProjectSet"/>.
        /// </summary>
        [Memo]
        public ImmutableArray<IProperty> Properties => this.Types.SelectMany( t => t.Properties ).ToImmutableArray();

        /// <summary>
        /// Gets all properties and properties in the current <see cref="ProjectSet"/>.
        /// </summary>
        [Memo]
        public ImmutableArray<IFieldOrProperty> FieldsAndProperties => this.Types.SelectMany( t => t.FieldsAndProperties ).ToImmutableArray();

        /// <summary>
        /// Gets all constructors in the current <see cref="ProjectSet"/>.
        /// </summary>
        [Memo]
        public ImmutableArray<IConstructor> Constructors => this.Types.SelectMany( t => t.Constructors ).ToImmutableArray();

        /// <summary>
        /// Gets all events in the current <see cref="ProjectSet"/>.
        /// </summary>
        [Memo]
        public ImmutableArray<IEvent> Events => this.Types.SelectMany( t => t.Events ).ToImmutableArray();

        /// <summary>
        /// Gets all diagnostics reported in the projects in the current <see cref="ProjectSet"/>. Note that the diagnostics reported
        /// by Caravela are not yet included in this set.
        /// </summary>
        [Memo]
        public ImmutableArray<DiagnosticModel> Diagnostics
            => this.Projects
                .SelectMany( p => p.Compilation.GetRoslynCompilation().GetDiagnostics().Select( d => new DiagnosticModel( d, p.Compilation ) ) )
                .ToImmutableArray();

        /// <summary>
        /// Returns a subset of the current subset.
        /// </summary>
        /// <param name="filter">A predicate that determines if a project must be a part of the new subset.</param>
        /// <returns></returns>
        public ProjectSet GetSubset( Predicate<Project> filter )
        {
            var filteredProjects = this.Projects.Where( p => filter( p ) ).OrderBy( p => p.ToString() ).ToImmutableArray();
            var filteredProjectKey = string.Join( "+", filteredProjects );

            return this._subsets.GetOrAdd( filteredProjectKey, _ => new ProjectSet( filteredProjects ) );
        }

        /// <summary>
        /// Gets a declaration in the current subset. 
        /// </summary>
        /// <param name="projectPath">Path of the project.</param>
        /// <param name="targetFramework">Target framework, or an empty string.</param>
        /// <param name="declarationId">Serialized identifier of the declaration obtained  with <see cref="IRef{T}.Serialize"/>.</param>
        /// <returns></returns>
        public IDeclaration? GetDeclaration( string projectPath, string targetFramework, string declarationId )
        {
            var projects = this.Projects
                .Where( p => p.Path == projectPath && (string.IsNullOrEmpty( targetFramework ) || p.TargetFramework == targetFramework) )
                .ToList();

            switch ( projects.Count )
            {
                case 0:
                    throw new InvalidOperationException( "The current ProjectSet does not contain a project with matching path and target framework." );

                case >1:
                    throw new InvalidOperationException( "The project targets several frameworks. Specify the target framework." );
            }

            return projects[0].Compilation.TypeFactory.GetDeclarationFromId( declarationId );
        }
    }
}