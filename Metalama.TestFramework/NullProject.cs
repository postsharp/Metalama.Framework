// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.TestFramework
{
    internal class NullProject : IProject
    {
        public NullProject( ProjectServiceProvider serviceProvider ) { this.ServiceProvider = serviceProvider; }

        public string Name => throw new NotImplementedException();

        public string Path => throw new NotImplementedException();

        public ImmutableArray<IAssemblyIdentity> AssemblyReferences => throw new NotImplementedException();

        public ImmutableHashSet<string> PreprocessorSymbols => throw new NotImplementedException();

        public string? Configuration => throw new NotImplementedException();

        public string? TargetFramework => throw new NotImplementedException();

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => throw new NotImplementedException();

        public T Extension<T>()
            where T : ProjectExtension, new()
            => throw new NotImplementedException();

        IServiceProvider<IProjectService> IProject.ServiceProvider => this.ServiceProvider.Underlying;

        public ProjectServiceProvider ServiceProvider { get; }
    }
}