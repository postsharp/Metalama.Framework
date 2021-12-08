// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.Options;
using Metalama.Framework.Impl.Pipeline;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Impl.CodeModel
{
    internal class ProjectModel : IProject
    {
        private readonly ConcurrentDictionary<Type, ProjectExtension> _extensions = new();
        private readonly IProjectOptions _projectOptions;
        private readonly Lazy<ImmutableArray<IAssemblyIdentity>> _projectReferences;
        private bool _isFrozen;

        public ProjectModel( Compilation compilation, IServiceProvider serviceProvider )
        {
            var serviceProviderMetadata = serviceProvider.GetService<ServiceProviderMark>();

            if ( serviceProviderMetadata != ServiceProviderMark.Project && serviceProviderMetadata != ServiceProviderMark.Test )
            {
                // We should get a project-specific service provider here, except in unit tests, but not a global or pipeline one.
                throw new ArgumentOutOfRangeException( nameof(serviceProvider) );
            }

            this._projectOptions = serviceProvider.GetService<IProjectOptions>();
            var anySyntaxTree = compilation.SyntaxTrees.FirstOrDefault();

            this.PreprocessorSymbols =
                anySyntaxTree != null ? anySyntaxTree.Options.PreprocessorSymbolNames.ToImmutableHashSet() : ImmutableHashSet<string>.Empty;

            this.ServiceProvider = serviceProvider;

            this._projectReferences =
                new Lazy<ImmutableArray<IAssemblyIdentity>>(
                    () => compilation.ReferencedAssemblyNames.Select( a => new AssemblyIdentityModel( a ) ).ToImmutableArray<IAssemblyIdentity>() );
        }

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
            var data = (ProjectExtension) Activator.CreateInstance( t );
            data.Initialize( this, this._isFrozen );

            return data;
        }

        public IServiceProvider ServiceProvider { get; }

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
    }
}