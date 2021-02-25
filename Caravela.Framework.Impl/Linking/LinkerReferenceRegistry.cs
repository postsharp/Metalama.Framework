using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerReferenceRegistry
    {
        private readonly LinkerTransformationRegistry _transformationRegistry;

        public bool IsOverrideTarget( IMethodSymbol? symbol )
        {
            throw new NotImplementedException();
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

        internal bool HasSimpleReturn( IMethodSymbol contextSymbol )
        {
            throw new NotImplementedException();
        }
    }
}
