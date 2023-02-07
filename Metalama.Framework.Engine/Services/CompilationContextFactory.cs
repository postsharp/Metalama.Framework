// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Services
{
    public static class CompilationContextFactory
    {
        // This should be used only for tests.
        internal static Compilation EmptyCompilation { get; } = CSharpCompilation.Create( "<empty>" );

        // We need a WeakCache because of DesignTimeAspectPipeline, which stores the ReflectionMapperFactory service for a long time.

        private static readonly WeakCache<Compilation, CompilationContext> _instances = new();

        public static CompilationContext GetInstance( Compilation compilation ) => _instances.GetOrAdd( compilation, c => new CompilationContext( c ) );
    }
}