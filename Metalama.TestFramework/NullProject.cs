// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;

namespace Metalama.TestFramework
{
    internal class NullProject : IProject
    {
        public NullProject( IServiceProvider serviceProvider ) { this.ServiceProvider = serviceProvider; }

        public string Path => throw new NotImplementedException();

        public ImmutableArray<IAssemblyIdentity> AssemblyReferences => throw new NotImplementedException();

        public ImmutableHashSet<string> PreprocessorSymbols => throw new NotImplementedException();

        public string? Configuration => throw new NotImplementedException();

        public string? TargetFramework => throw new NotImplementedException();

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => throw new NotImplementedException();

        public T Extension<T>()
            where T : ProjectExtension, new()
            => throw new NotImplementedException();

        public IServiceProvider ServiceProvider { get; }
    }
}