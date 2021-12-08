// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.CodeModel.Builders;
using Metalama.Framework.Impl.Templating;
using Metalama.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.Linking
{
    internal sealed class LexicalScopeFactory : ITemplateLexicalScopeProvider
    {
        private readonly Dictionary<IDeclaration, TemplateLexicalScope> _scopes;

        public LexicalScopeFactory( CompilationModel compilation )
        {
            this._scopes = new Dictionary<IDeclaration, TemplateLexicalScope>( compilation.InvariantComparer );
        }

        public TemplateLexicalScope GetLexicalScope( IDeclaration declaration )
        {
            if ( !this._scopes.TryGetValue( declaration, out var lexicalScope ) )
            {
                switch ( declaration )
                {
                    case Method { ContainingDeclaration: Event } sourceAccessor:
                        this._scopes[declaration] = lexicalScope = new TemplateLexicalScope( sourceAccessor.LookupSymbols() );

                        break;

                    case Method { ContainingDeclaration: Property } sourceAccessor:
                        this._scopes[declaration] = lexicalScope = new TemplateLexicalScope( sourceAccessor.LookupSymbols() );

                        break;

                    case Method sourceMethod:
                        this._scopes[declaration] = lexicalScope = new TemplateLexicalScope( sourceMethod.LookupSymbols() );

                        break;

                    case MethodBuilder { DeclaringType: NamedType containingType }:
                        this._scopes[declaration] = lexicalScope = new TemplateLexicalScope( containingType.LookupSymbols() );

                        break;

                    case IMethod { ContainingDeclaration: MemberBuilder { DeclaringType: NamedType containingType } _ }:
                        this._scopes[declaration] = lexicalScope = new TemplateLexicalScope( containingType.LookupSymbols() );

                        break;

                    default:
                        // GetLexicalScope must be called first with the ICodeElementBuilder. In this flow,
                        // we don't have the target SyntaxTree, so we cannot compute the lexical scope.
                        throw new AssertionFailedException();
                }
            }

            return lexicalScope;
        }
    }
}