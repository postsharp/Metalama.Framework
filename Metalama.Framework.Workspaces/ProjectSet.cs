// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Workspaces
{
    internal sealed class ProjectSet : CompilationSet, IProjectSet
    {
        private readonly ConcurrentDictionary<string, ProjectSet> _subsets = new();

        /// <summary>
        /// Gets the projects in the current <see cref="ProjectSet"/>.
        /// </summary>
        public ImmutableArray<Project> Projects { get; }

        internal ProjectSet( ImmutableArray<Project> projects, string name ) : base( name, projects.Select( x => x.Compilation ).ToImmutableArray() )
        {
            // This gets a snapshot of the collection of project at the moment the object is created.
            this.Projects = projects;
        }

        [Memo]
        public override ImmutableArray<IIntrospectionDiagnostic> SourceDiagnostics
            => this.Projects
                .SelectMany( p => p.SourceDiagnostics )
                .ToImmutableArray();

        public IProjectSet GetSubset( Predicate<Project> filter )
        {
            var filteredProjects = this.Projects.Where( p => filter( p ) ).OrderBy( p => p.ToString() ).ToImmutableArray();
            var filteredProjectKey = string.Join( "+", filteredProjects );

            return this._subsets.GetOrAdd( filteredProjectKey, _ => new ProjectSet( filteredProjects, $"Subset of {this}" ) );
        }

        public IDeclaration? GetDeclaration( string projectPath, string targetFramework, string declarationId, bool metalamaOutput )
        {
            var projects = this.Projects
                .Where( p => p.Path == projectPath && (string.IsNullOrEmpty( targetFramework ) || p.TargetFramework == targetFramework) )
                .ToList();

            switch ( projects.Count )
            {
                case 0:
                    throw new InvalidOperationException( "The current ProjectSet does not contain a project with matching path and target framework." );

                case > 1:
                    throw new InvalidOperationException( "The project targets several frameworks. Specify the target framework." );
            }

            var compilation = metalamaOutput ? projects[0].MetalamaOutput.Compilation : projects[0].Compilation;

            return compilation.GetDeclarationFromId( new DeclarationSerializableId( declarationId ) );
        }

        [Memo]
        public IMetalamaCompilationSet MetalamaOutput
            => new MetalamaCompilationSet( this.Projects.Select( x => x.MetalamaOutput ).ToImmutableArray(), this.ToString() );
    }
}