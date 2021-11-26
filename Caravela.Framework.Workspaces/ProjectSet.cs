// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Impl.Utilities.Dump;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Workspaces
{
    public sealed class ProjectSet
    {
        private readonly ConcurrentDictionary<string, ProjectSet> _subsets = new();

        public ImmutableArray<Project> Projects { get; }

        internal ProjectSet( ImmutableArray<Project> projects )
        {
            // This gets a snapshot of the collection of project at the moment the object is created.
            this.Projects = projects;
        }

        [Memo]
        public ImmutableArray<INamedType> Types => this.Projects.SelectMany( p => p.Types ).ToImmutableArray();

        [Memo]
        public ImmutableArray<TargetFramework> TargetFrameworks
            => this.Projects.Select( p => p.TargetFramework )
                .WhereNotNull()
                .Distinct()
                .OrderBy( s => s )
                .Select( s => new TargetFramework( s ) )
                .ToImmutableArray();

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

        public ProjectSet GetSubset( Predicate<Project> filter )
        {
            var filteredProjects = this.Projects.Where( p => filter( p ) ).OrderBy( p => p.ToString() ).ToImmutableArray();
            var filteredProjectKey = string.Join( "+", filteredProjects );

            return this._subsets.GetOrAdd( filteredProjectKey, _ => new ProjectSet( filteredProjects ) );
        }

        // ReSharper disable once UnusedMember.Local
        private object? ToDump() => ObjectDumper.Dump( this );
    }
}