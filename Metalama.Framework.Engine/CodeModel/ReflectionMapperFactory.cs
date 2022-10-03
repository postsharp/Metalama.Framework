// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class ReflectionMapperFactory : IService
    {
        // We need a ConditionalWeakTable because of DesignTimeAspectPipeline, which stores the ReflectionMapperFactory service for a long time.

#pragma warning disable CA1805 // Do not initialize unnecessarily
        private readonly WeakCache<Compilation, ReflectionMapper> _instances = new();
#pragma warning restore CA1805 // Do not initialize unnecessarily

        /// <summary>
        /// Gets a <see cref="ReflectionMapper"/> instance for a given <see cref="Compilation"/>.
        /// </summary>
        public ReflectionMapper GetInstance( Compilation compilation ) => this._instances.GetOrAdd( compilation, c => new ReflectionMapper( c ) );
    }
}