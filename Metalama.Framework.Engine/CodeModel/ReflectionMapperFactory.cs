// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class ReflectionMapperFactory : IService
    {
        // We need a ConditionalWeakTable because of DesignTimeAspectPipeline, which stores the ReflectionMapperFactory service for a long time.

        private readonly ConditionalWeakTable<Compilation, ReflectionMapper> _instances = new();

        /// <summary>
        /// Gets a <see cref="ReflectionMapper"/> instance for a given <see cref="Compilation"/>.
        /// </summary>
        public ReflectionMapper GetInstance( Compilation compilation )
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !this._instances.TryGetValue( compilation, out var value ) )
            {
                lock ( this._instances )
                {
                    if ( !this._instances.TryGetValue( compilation, out value ) )
                    {
                        value = new ReflectionMapper( compilation );
                        this._instances.Add( compilation, value );
                    }
                }
            }

            return value;
        }
    }
}