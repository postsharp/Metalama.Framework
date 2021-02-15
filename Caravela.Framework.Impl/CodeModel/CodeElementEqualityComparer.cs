using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.CodeModel
{
    public class CodeElementEqualityComparer : IEqualityComparer<ICodeElement>
    {
        public static readonly CodeElementEqualityComparer Instance = new CodeElementEqualityComparer();

        public bool Equals( ICodeElement x, ICodeElement y )
        {
            if ( x is CodeElement cx && y is CodeElement cy )
            {
                return SymbolEqualityComparer.Default.Equals( cx.Symbol, cy.Symbol );
            }
            else
            {
                return x == y;
            }
        }

        public int GetHashCode( ICodeElement x )
        {
            if ( x is CodeElement cx )
            {
                return SymbolEqualityComparer.Default.GetHashCode( cx.Symbol );
            }
            else
            {
                return x.GetHashCode();
            }
        }
    }
}
