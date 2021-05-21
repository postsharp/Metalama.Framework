// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl
{
    internal class AspectDriverFactory
    {
        private readonly Compilation _compilation;
        private readonly ILookup<string, IAspectWeaver> _weaverTypes;

        public AspectDriverFactory( Compilation compilation, ImmutableArray<object> plugins )
        {
            this._compilation = compilation;

            this._weaverTypes = plugins.OfType<IAspectWeaver>()
                .ToLookup( weaver => weaver.GetType().GetCustomAttribute<AspectWeaverAttribute>().AspectType.FullName );
        }

        public IAspectDriver GetAspectDriver( INamedTypeSymbol type )
        {
            var weavers = this._weaverTypes[type.GetReflectionNameSafe()].ToList();

            if ( weavers.Count > 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectHasMoreThanOneWeaver.CreateException( (type, string.Join( ", ", weavers )) );
            }

            if ( weavers.Count == 1 )
            {
                return weavers.Single();
            }

            return new AspectDriver( type, this._compilation );
        }
    }
}