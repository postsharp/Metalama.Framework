using Caravela.Framework.Code;
using System;
using System.Collections.Immutable;

namespace Caravela.TestFramework
{
    internal class NullProject : IProject
    {
        public static IProject Instance { get; } = new NullProject();
        private NullProject() { }

        public string Path => throw new NotImplementedException();

        public ImmutableHashSet<string> DefinedSymbols => throw new NotImplementedException();

        public string? Configuration => throw new NotImplementedException();

        public string? TargetFramework => throw new NotImplementedException();

        public bool TryGetProperty( string name, out string? value ) => throw new NotImplementedException();

        public T Extension<T>() where T : IProjectExtension, new() => throw new NotImplementedException();

        public IServiceProvider ServiceProvider => throw new NotImplementedException();
    }
}