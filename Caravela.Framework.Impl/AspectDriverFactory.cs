// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl
{
    internal class AspectDriverFactory
    {
        private readonly CompilationModel _compilation;
        private readonly ILookup<string, IAspectWeaver> _weaverTypes;

        public AspectDriverFactory( CompilationModel compilation, ImmutableArray<object> plugins )
        {
            this._compilation = compilation;

            this._weaverTypes = plugins.OfType<IAspectWeaver>()
                                       .ToLookup( weaver => weaver.GetType().GetCustomAttribute<AspectWeaverAttribute>().AspectType.FullName );
        }

        public IAspectDriver GetAspectDriver( INamedType type )
        {
            var weavers = this._weaverTypes[type.FullName].ToList();

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