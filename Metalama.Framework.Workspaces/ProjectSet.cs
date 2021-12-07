// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Workspaces
{
    internal sealed class ProjectSet : IProjectSet
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

        /// <inheritdoc />
        [Memo]
        public ImmutableArray<TargetFramework> TargetFrameworks
            => this.Projects.Select( p => p.TargetFramework )
                .Where( t => t.Id != null )
                .Distinct()
                .OrderBy( s => s.Id )
                .ToImmutableArray();

        [Memo]
        public ImmutableArray<INamedType> Types => this.Projects.SelectMany( p => p.Types ).ToImmutableArray();

        [Memo]
        public ImmutableArray<IMethod> Methods => this.Types.SelectMany( t => t.Methods ).ToImmutableArray();

        [Memo]
        public ImmutableArray<IField> Fields => this.Types.SelectMany( t => t.Fields ).ToImmutableArray();

        [Memo]
        public ImmutableArray<IProperty> Properties => this.Types.SelectMany( t => t.Properties ).ToImmutableArray();

        [Memo]
        public ImmutableArray<IFieldOrProperty> FieldsAndProperties => this.Types.SelectMany( t => t.FieldsAndProperties ).ToImmutableArray();

        [Memo]
        public ImmutableArray<IConstructor> Constructors => this.Types.SelectMany( t => t.Constructors ).ToImmutableArray();

        [Memo]
        public ImmutableArray<IEvent> Events => this.Types.SelectMany( t => t.Events ).ToImmutableArray();

        [Memo]
        public ImmutableArray<IDiagnostic> Diagnostics
            => this.Projects
                .SelectMany( p => p.Compilation.GetRoslynCompilation().GetDiagnostics().Select( d => new DiagnosticModel( d, p.Compilation ) ) )
                .ToImmutableArray<IDiagnostic>();

        public IProjectSet GetSubset( Predicate<Project> filter )
        {
            var filteredProjects = this.Projects.Where( p => filter( p ) ).OrderBy( p => p.ToString() ).ToImmutableArray();
            var filteredProjectKey = string.Join( "+", filteredProjects );

            return this._subsets.GetOrAdd( filteredProjectKey, _ => new ProjectSet( filteredProjects ) );
        }

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