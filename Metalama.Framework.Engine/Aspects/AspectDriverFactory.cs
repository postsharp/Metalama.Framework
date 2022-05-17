// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectWeavers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Creates instances of <see cref="IAspectDriver"/> for a given <see cref="AspectClass"/>.
    /// </summary>
    internal class AspectDriverFactory
    {
        private readonly Compilation _compilation;
        private readonly IServiceProvider _serviceProvider;
        private readonly ImmutableDictionary<string, IAspectDriver> _weaverTypes;

        public AspectDriverFactory( Compilation compilation, ImmutableArray<object> plugins, IServiceProvider serviceProvider )
        {
            this._compilation = compilation;
            this._serviceProvider = serviceProvider;

            this._weaverTypes = plugins.OfType<IAspectDriver>()
                .ToImmutableDictionary( weaver => weaver.GetType().FullName );
        }

        public IAspectDriver GetAspectDriver( AspectClass aspectClass )
        {
            if ( aspectClass.WeaverType != null )
            {
                if ( !this._weaverTypes.TryGetValue( aspectClass.WeaverType, out var registeredAspectDriver ) )
                {
                    // It's okay to have a missing driver if the aspect is not instantiated.
                    // This is actually a common situation when building the project defining the aspect class.
                    // Return an ErrorAspectWeaver that will emit an error when used.
                    return new ErrorAspectWeaver( aspectClass );
                }

                return registeredAspectDriver;
            }

            return new AspectDriver( this._serviceProvider, aspectClass, this._compilation );
        }
    }
}