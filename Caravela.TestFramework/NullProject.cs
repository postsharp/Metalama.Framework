// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.TestFramework
{
    internal class NullProject : IProject
    {
        public static IProject Instance { get; } = new NullProject();

        private NullProject() { }

        public string Path => throw new NotImplementedException();

        public ImmutableArray<IAssemblyIdentity> AssemblyReferences => throw new NotImplementedException();

        public ImmutableHashSet<string> DefinedSymbols => throw new NotImplementedException();

        public string? Configuration => throw new NotImplementedException();

        public string? TargetFramework => throw new NotImplementedException();

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => throw new NotImplementedException();

        public T Extension<T>() 
            where T : IProjectExtension, new()
            => throw new NotImplementedException();

        public IServiceProvider ServiceProvider => throw new NotImplementedException();
    }
}