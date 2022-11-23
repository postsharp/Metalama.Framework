// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    public class ProjectModel : IProject
    {
        private readonly ConcurrentDictionary<Type, ProjectExtension> _extensions = new();
        private readonly IProjectOptions _projectOptions;
        private readonly Lazy<ImmutableArray<IAssemblyIdentity>> _projectReferences;
        private bool _isFrozen;

        public ProjectModel( Compilation compilation, ProjectServiceProvider serviceProvider )
        {
            this._projectOptions = serviceProvider.GetRequiredService<IProjectOptions>();
            var anySyntaxTree = compilation.SyntaxTrees.FirstOrDefault();

            this.PreprocessorSymbols =
                anySyntaxTree != null ? anySyntaxTree.Options.PreprocessorSymbolNames.ToImmutableHashSet() : ImmutableHashSet<string>.Empty;

            this.ServiceProvider = serviceProvider.Underlying;

            this._projectReferences =
                new Lazy<ImmutableArray<IAssemblyIdentity>>(
                    () => compilation.ReferencedAssemblyNames.Select( a => new AssemblyIdentityModel( a ) ).ToImmutableArray<IAssemblyIdentity>() );
        }

        [Memo]
        public string Name
            => this._projectOptions.ProjectPath != null
                ? System.IO.Path.GetFileNameWithoutExtension( this._projectOptions.ProjectPath )
                : this._projectOptions.AssemblyName ?? "unnamed";

        public string? Path => this._projectOptions.ProjectPath;

        public ImmutableArray<IAssemblyIdentity> AssemblyReferences => this._projectReferences.Value;

        public ImmutableHashSet<string> PreprocessorSymbols { get; }

        public string? Configuration => this._projectOptions.Configuration;

        public string? TargetFramework => this._projectOptions.TargetFramework;

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => this._projectOptions.TryGetProperty( name, out value );

        public T Extension<T>()
            where T : ProjectExtension, new()
            => (T) this._extensions.GetOrAdd( typeof(T), this.CreateProjectExtension );

        private ProjectExtension CreateProjectExtension( Type t )
        {
            var data = (ProjectExtension) Activator.CreateInstance( t ).AssertNotNull();
            data.Initialize( this, this._isFrozen );

            return data;
        }

        public ProjectServiceProvider ServiceProvider { get; }

        IServiceProvider<IProjectService> IProject.ServiceProvider => this.ServiceProvider.Underlying;

        internal void Freeze()
        {
            if ( this._isFrozen )
            {
                return;
            }

            this._isFrozen = true;

            foreach ( var data in this._extensions.Values )
            {
                data.MakeReadOnly();

                // Also set the property explicitly in case an implementer skips the call to base.MakeReadOnly.
                data.IsReadOnly = true;
            }
        }

        public override string ToString() => this.Path ?? "(no project path)";
    }
}