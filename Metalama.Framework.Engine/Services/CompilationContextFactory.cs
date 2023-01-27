// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.Services
{
    public sealed class CompilationContextFactory : IProjectService, IDisposable
    {
        // This should be used only for tests.
        internal static Compilation EmptyCompilation { get; } = CSharpCompilation.Create( "<empty>" );

        private readonly ProjectServiceProvider _serviceProvider;

        // We need a ConditionalWeakTable because of DesignTimeAspectPipeline, which stores the ReflectionMapperFactory service for a long time.

        private readonly WeakCache<Compilation, CompilationContext> _instances = new();

        internal CompilationContextFactory( ProjectServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider.Underlying.WithService( this );
        }

        /// <summary>
        /// Gets a <see cref="ReflectionMapper"/> instance for a given <see cref="Compilation"/>.
        /// </summary>
        public CompilationContext GetInstance( Compilation compilation )
            => this._instances.GetOrAdd( compilation, c => new CompilationContext( c, this._serviceProvider, this ) );

        [Memo]
        internal CompilationContext Empty => new( EmptyCompilation, this._serviceProvider, this );

        public void Dispose() => this._instances.Dispose();
    }
}