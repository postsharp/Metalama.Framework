// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Services
{
    public class CompilationContextFactory : IProjectService
    {
        // This should be used only for tests.
        public static Compilation EmptyCompilation { get; } = CSharpCompilation.Create( "<empty>" );

        private readonly ProjectServiceProvider _serviceProvider;

        // We need a ConditionalWeakTable because of DesignTimeAspectPipeline, which stores the ReflectionMapperFactory service for a long time.

        private readonly WeakCache<Compilation, CompilationContext> _instances = new();

        public CompilationContextFactory( ProjectServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider.Underlying.WithService( this );
        }

        /// <summary>
        /// Gets a <see cref="ReflectionMapper"/> instance for a given <see cref="Compilation"/>.
        /// </summary>
        public CompilationContext GetInstance( Compilation compilation )
            => this._instances.GetOrAdd( compilation, c => new CompilationContext( c, this._serviceProvider ) );

        [Memo]
        public CompilationContext Empty => new( EmptyCompilation, this._serviceProvider );
    }
}