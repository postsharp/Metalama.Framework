// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal class LexicalScopeFactory
    {
        private readonly CompilationModel _compilation;
        private readonly Dictionary<IDeclaration, TemplateLexicalScope> _scopes;

        public LexicalScopeFactory( CompilationModel compilation )
        {
            this._compilation = compilation;
            this._scopes = new Dictionary<IDeclaration, TemplateLexicalScope>( compilation.InvariantComparer );
        }

        public TemplateLexicalScope GetLexicalScope( IDeclaration declaration )
        {
            if ( !this._scopes.TryGetValue( declaration, out var lexicalScope ) )
            {
                switch ( declaration )
                {
                    case Declaration sourceCodeElement:
                        this._scopes[sourceCodeElement] = lexicalScope = new TemplateLexicalScope( sourceCodeElement.LookupSymbols() );

                        break;

                    default:
                        // GetLexicalScope must be called first with the ICodeElementBuilder. In this flow,
                        // we don't have the target SyntaxTree, so we cannot compute the lexical scope.
                        throw new AssertionFailedException();
                }
            }

            return lexicalScope;
        }

        public TemplateLexicalScope GetLexicalScope( IMemberIntroduction introduction )
        {
            // TODO: This will need to be changed for other transformations than methods.

            switch ( introduction )
            {
                case IOverriddenElement overriddenElement:
                    {
                        // When we have an IOverriddenElement, we know which symbol will be overwritten, so we take its lexical scope.
                        // All overrides of these same symbol will share the same scope.
                        return this.GetLexicalScope( overriddenElement.OverriddenElement );
                    }

                case IDeclaration declaration:
                    {
                        // We have a member introduction.
                        if ( !this._scopes.TryGetValue( declaration, out var lexicalScope ) )
                        {
                            // The lexical scope should be taken from the insertion point.

                            var semanticModel = this._compilation.RoslynCompilation.GetSemanticModel( introduction.TargetSyntaxTree );

                            int lookupPosition;

                            switch ( introduction.InsertPositionNode )
                            {
                                case TypeDeclarationSyntax type:
                                    // The lookup position is inside the type.
                                    lookupPosition = type.CloseBraceToken.SpanStart - 1;

                                    break;

                                default:
                                    // The lookup position is after the member.
                                    lookupPosition = introduction.InsertPositionNode.Span.End + 1;

                                    break;
                            }

                            this._scopes[declaration] = lexicalScope =
                                new TemplateLexicalScope( semanticModel.LookupSymbols( lookupPosition ) );
                        }

                        return lexicalScope;
                    }

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}