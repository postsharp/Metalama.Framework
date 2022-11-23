// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    public class CompilationServicesFactory : IProjectService
    {
        // This should be used only for tests.
        public static Compilation EmptyCompilation { get; } = CSharpCompilation.Create( "<empty>" );

        private readonly ProjectServiceProvider _serviceProvider;

        // We need a ConditionalWeakTable because of DesignTimeAspectPipeline, which stores the ReflectionMapperFactory service for a long time.

        private readonly WeakCache<Compilation, CompilationServices> _instances = new();

        public CompilationServicesFactory( ProjectServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider.Underlying.WithService( this );
        }

        /// <summary>
        /// Gets a <see cref="ReflectionMapper"/> instance for a given <see cref="Compilation"/>.
        /// </summary>
        public CompilationServices GetInstance( Compilation compilation )
            => this._instances.GetOrAdd( compilation, c => new CompilationServices( c, this._serviceProvider ) );

        [Memo]
        public CompilationServices Empty => new( EmptyCompilation, this._serviceProvider );
    }
}