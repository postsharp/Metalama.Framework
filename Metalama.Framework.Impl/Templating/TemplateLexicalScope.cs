// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Impl.Templating
{
    internal class TemplateLexicalScope
    {
        private readonly HashSet<string> _symbols;

        public TemplateLexicalScope( IEnumerable<ISymbol> symbols )
        {
            this._symbols = new HashSet<string>( symbols.Select( x => x.Name ) );
        }

        public string GetUniqueIdentifier( string hint )
        {
            if ( this._symbols.Add( hint ) )
            {
                return hint;
            }

            string name;

            for ( var i = 1; !this._symbols.Add( name = hint + "_" + i ); i++ )
            {
                // Intentionally empty.
            }

            return name;
        }
    }
}