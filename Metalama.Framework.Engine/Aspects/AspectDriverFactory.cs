// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectWeavers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Creates instances of <see cref="IAspectDriver"/> for a given <see cref="AspectClass"/>.
    /// </summary>
    internal class AspectDriverFactory
    {
        private readonly Compilation _compilation;
        private readonly IServiceProvider _serviceProvider;
        private readonly ImmutableDictionary<string, IAspectWeaver> _weaverTypes;

        public AspectDriverFactory( Compilation compilation, ImmutableArray<object> plugins, IServiceProvider serviceProvider )
        {
            this._compilation = compilation;
            this._serviceProvider = serviceProvider;

            this._weaverTypes = plugins.OfType<IAspectWeaver>()
                .ToImmutableDictionary( weaver => weaver.GetType().FullName );
        }

        public IAspectDriver GetAspectDriver( AspectClass aspectClass, INamedTypeSymbol type )
        {
            if ( aspectClass.WeaverType != null )
            {
                if ( !this._weaverTypes.TryGetValue( aspectClass.WeaverType, out var weaver ) )
                {
                    throw new InvalidOperationException(
                        $"The weaver type '{aspectClass.WeaverType}' required to weave aspect '{aspectClass.ShortName}' is not found in the project. The weaver assembly must be included as an analyzer." );
                }

                return weaver;
            }

            return new AspectDriver( this._serviceProvider, aspectClass, this._compilation );
        }
    }
}