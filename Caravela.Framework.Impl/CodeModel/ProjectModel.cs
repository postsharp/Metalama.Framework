// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class ProjectModel : IProject
    {
        private readonly ConcurrentDictionary<Type, object> _extensions = new();
        private readonly IProjectOptions _projectOptions;
        private readonly SyntaxTree? _anySyntaxTree;
        private readonly Lazy<ImmutableArray<IAssemblyIdentity>> _projectReferences;

        public ProjectModel( Compilation compilation, IServiceProvider serviceProvider )
        {
            var serviceProviderMetadata = serviceProvider.GetService<ServiceProviderMark>();

            if ( serviceProviderMetadata != ServiceProviderMark.Project && serviceProviderMetadata != ServiceProviderMark.Test )
            {
                // We should get a project-specific service provider here, except in unit tests, but not a global or pipeline one.
                throw new ArgumentOutOfRangeException( nameof(serviceProvider) );
            }

            this._projectOptions = serviceProvider.GetService<IProjectOptions>();
            this._anySyntaxTree = compilation.SyntaxTrees.FirstOrDefault();
            this.ServiceProvider = serviceProvider;

            this._projectReferences =
                new Lazy<ImmutableArray<IAssemblyIdentity>>(
                    () => compilation.ReferencedAssemblyNames.Select( a => new AssemblyIdentityModel( a ) ).ToImmutableArray<IAssemblyIdentity>() );
        }

        public string? Path => this._projectOptions.ProjectPath;

        public ImmutableArray<IAssemblyIdentity> AssemblyReferences => this._projectReferences.Value;

        [Memo]
        public ImmutableHashSet<string> DefinedSymbols
            => this._anySyntaxTree != null ? this._anySyntaxTree.Options.PreprocessorSymbolNames.ToImmutableHashSet() : ImmutableHashSet<string>.Empty;

        public string? Configuration => this._projectOptions.Configuration;

        public string? TargetFramework => this._projectOptions.TargetFramework;

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => this._projectOptions.TryGetProperty( name, out value );

        public T Data<T>()
            where T : class, IProjectData, new()
            => (T) this._extensions.GetOrAdd( typeof(T), this.CreateProjectData );

        private object CreateProjectData( Type t )
        {
            var data = (IProjectData) Activator.CreateInstance( t );
            data.Initialize( this );

            return data;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}