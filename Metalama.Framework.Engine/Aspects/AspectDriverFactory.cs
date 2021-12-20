// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Creates instances of <see cref="IAspectDriver"/> for a given <see cref="AspectClass"/>.
    /// </summary>
    internal class AspectDriverFactory
    {
        private readonly Compilation _compilation;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILookup<string, IAspectWeaver> _weaverTypes;

        public AspectDriverFactory( Compilation compilation, ImmutableArray<object> plugins, IServiceProvider serviceProvider )
        {
            this._compilation = compilation;
            this._serviceProvider = serviceProvider;

            this._weaverTypes = plugins.OfType<IAspectWeaver>()
                .ToLookup( weaver => weaver.GetType().GetCustomAttribute<AspectWeaverAttribute>().AspectType.FullName );
        }

        public IAspectDriver GetAspectDriver( AspectClass aspectClass, INamedTypeSymbol type )
        {
            var weavers = this._weaverTypes[type.GetReflectionName().AssertNotNull(  )].ToList();

            if ( weavers.Count > 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectHasMoreThanOneWeaver.CreateException( (type, string.Join( ", ", weavers )) );
            }

            if ( weavers.Count == 1 )
            {
                return weavers.Single();
            }

            return new AspectDriver( this._serviceProvider, aspectClass, this._compilation );
        }
    }
}