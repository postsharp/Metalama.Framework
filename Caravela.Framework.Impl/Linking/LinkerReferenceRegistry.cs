using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerReferenceRegistry
    {
        private LinkerTransformationRegistry _transformationRegistry;

        public bool IsOverrideTarget( IMethodSymbol? symbol )
        {
        }

        public bool IsBodyInlineable( IMethodSymbol? symbol )
        {
            throw new NotImplementedException();
        }

        internal bool IsOverrideMethod( IMethodSymbol? symbol )
        {
            throw new NotImplementedException();
        }

        internal ISymbol ResolveSymbolReference( IMethodSymbol contextSymbol, ISymbol calleeSymbol, LinkerAnnotation annotation )
        {
            throw new NotImplementedException();
        }
    }
}
