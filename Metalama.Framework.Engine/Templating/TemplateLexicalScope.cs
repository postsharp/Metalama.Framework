// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

            for ( var i = 1; /* Intentionally empty */; i++ )
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