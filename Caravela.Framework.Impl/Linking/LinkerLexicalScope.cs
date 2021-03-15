// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerLexicalScope : ITemplateExpansionLexicalScope
    {
        private readonly Dictionary<string, string> _identifierMap = new Dictionary<string, string>();
        private readonly HashSet<string> _definedIdentifiers = new HashSet<string>();
        private readonly ITemplateExpansionLexicalScope? _parentScope;
        private readonly List<LinkerLexicalScope> _nestedScopes = new List<LinkerLexicalScope>();

        public ITemplateExpansionLexicalScope? Parent { get; }

        public IReadOnlyCollection<string> DefinedIdentifiers => this._definedIdentifiers;

        public IReadOnlyList<ITemplateExpansionLexicalScope> NestedScopes => this._nestedScopes;

        private LinkerLexicalScope( IEnumerable<string> symbolNames )
        {
            this._parentScope = null;

            foreach ( var symbolName in symbolNames )
            {
                this._definedIdentifiers.Add( symbolName );
            }
        }

        private LinkerLexicalScope( ITemplateExpansionLexicalScope? parentLexicalScope )
        {
            this._parentScope = parentLexicalScope;
        }

        public void Dispose()
        {
        }

        public string DefineIdentifier( string name )
        {
            var targetName = name;
            var i = 0;
            while ( this.IsDefined( targetName ) )
            {
                i++;
                targetName = $"{name}_{i}";
            }

            this._definedIdentifiers.Add( targetName );
            this._identifierMap[name] = targetName;

            return targetName;
        }

        public string LookupIdentifier( string name )
        {
            if ( this._identifierMap.TryGetValue( name, out var targetName ) )
            {
                return targetName;
            }

            if ( this._parentScope != null )
            {
                return this._parentScope.LookupIdentifier( name );
            }

            return name;
        }

        public ITemplateExpansionLexicalScope OpenNestedScope()
        {
            return this;
        }

        public bool IsDefined( string name, bool includeAncestorScopes = true )
        {
            if ( this._definedIdentifiers.Contains( name ) )
            {
                return true;
            }

            if ( includeAncestorScopes && this._parentScope != null )
            {
                return this._parentScope.IsDefined( name, true );
            }

            return false;
        }

        internal static LinkerLexicalScope CreateFromMethod( IMethodInternal overriddenElement )
        {
            // TODO: This probably does not handle namespaces?
            return new LinkerLexicalScope( overriddenElement.LookupSymbols().Select( x => x.Name ) );
        }

        internal static LinkerLexicalScope CreateEmpty( ITemplateExpansionLexicalScope? parent = null )
        {
            return new LinkerLexicalScope( parent );
        }

        internal LinkerLexicalScope GetTransitiveClosure()
        {
            return new LinkerLexicalScope( GetTransitiveClosureIdentifiers( this ) );

            static IEnumerable<string> GetTransitiveClosureIdentifiers( ITemplateExpansionLexicalScope scope )
            {
                foreach ( var identifier in scope.DefinedIdentifiers )
                {
                    yield return identifier;
                }

                foreach ( var nestedScope in scope.NestedScopes )
                {
                    foreach ( var identifier in GetTransitiveClosureIdentifiers( nestedScope ) )
                    {
                        yield return identifier;
                    }
                }
            }
        }
    }
}
