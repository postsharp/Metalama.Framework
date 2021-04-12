// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Linking
{

    internal partial class LinkerIntroductionStep
    {
        private class LexicalScopeHelper
        {
            private readonly Dictionary<ICodeElement, LinkerLexicalScope> _scopes = new Dictionary<ICodeElement, LinkerLexicalScope>();

            public ITemplateExpansionLexicalScope GetLexicalScope( IMemberIntroduction introduction )
            {
                // TODO: This will need to be changed for other transformations than methods.

                if ( introduction is IOverriddenElement overriddenElement )
                {
                    if ( !this._scopes.TryGetValue( overriddenElement.OverriddenElement, out var lexicalScope ) )
                    {
                        this._scopes[overriddenElement.OverriddenElement] = lexicalScope =
                            LinkerLexicalScope.CreateEmpty( LinkerLexicalScope.CreateFromMethod( (IMethodInternal) overriddenElement.OverriddenElement ) );

                        return lexicalScope;
                    }

                    this._scopes[overriddenElement.OverriddenElement] = lexicalScope = LinkerLexicalScope.CreateEmpty( lexicalScope.GetTransitiveClosure() );
                    return lexicalScope;
                }
                else
                {
                    // For other member types we need to create empty lexical scope.
                    return LinkerLexicalScope.CreateEmpty();
                }
            }
        }
    }
}
