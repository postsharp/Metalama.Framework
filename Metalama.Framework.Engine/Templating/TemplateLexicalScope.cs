// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Templating
{
    internal class TemplateLexicalScope
    {
        private ImmutableHashSet<string> _symbols;

        public TemplateLexicalScope( ImmutableHashSet<string> symbols )
        {
            this._symbols = symbols;
        }

        public string GetUniqueIdentifier( string hint )
        {
            if ( !this._symbols.Contains( hint ) )
            {
                this._symbols = this._symbols.Add( hint );

                return hint;
            }

            for ( var i = 1;; i++ )
            {
                var name = hint + "_" + i;

                if ( !this._symbols.Contains( name ) )
                {
                    this._symbols = this._symbols.Add( name );

                    return name;
                }
            }
        }
    }
}