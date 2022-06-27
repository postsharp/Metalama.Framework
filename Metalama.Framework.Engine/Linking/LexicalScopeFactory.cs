// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LexicalScopeFactory : ITemplateLexicalScopeProvider
    {
        private readonly Dictionary<IDeclaration, TemplateLexicalScope> _scopes;

        public LexicalScopeFactory( CompilationModel compilation )
        {
            this._scopes = new Dictionary<IDeclaration, TemplateLexicalScope>( compilation.InvariantComparer );
        }

        /// <summary>
        /// Gets a shared lexical code where consumers can add their own symbols.
        /// </summary>
        public TemplateLexicalScope GetLexicalScope( IDeclaration declaration )
        {
            if ( !this._scopes.TryGetValue( declaration, out var lexicalScope ) )
            {
                this._scopes[declaration] = lexicalScope = GetSourceLexicalScope( declaration );
            }

            return lexicalScope;
        }

        /// <summary>
        /// Gets the lexical scope from source code.
        /// </summary>
        internal static TemplateLexicalScope GetSourceLexicalScope( IDeclaration declaration )
        {
            var symbol = declaration.GetSymbol();

            if ( symbol == null )
            {
                return new TemplateLexicalScope( ImmutableHashSet<string>.Empty );
            }

            var builder = ImmutableHashSet.CreateBuilder<string>();

            var syntaxReference = symbol.GetPrimarySyntaxReference();

            // Event fields have accessors without declaring syntax references.
            if ( syntaxReference == null )
            {
                switch ( symbol )
                {
                    case IMethodSymbol { MethodKind: RoslynMethodKind.EventAdd or RoslynMethodKind.EventRemove } eventAccessorSymbol:
                        syntaxReference = eventAccessorSymbol.AssociatedSymbol.AssertNotNull().GetPrimarySyntaxReference();

                        if ( syntaxReference == null )
                        {
                            throw new AssertionFailedException();
                        }

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }

            var semanticModel = declaration.GetCompilationModel().RoslynCompilation.GetSemanticModel( syntaxReference.SyntaxTree );

            // Accessors have implicit "value" parameter.
            if ( symbol is IMethodSymbol { MethodKind: RoslynMethodKind.PropertySet or RoslynMethodKind.EventAdd or RoslynMethodKind.EventRemove } )
            {
                builder.Add( "value" );
            }

            // Get the symbols defined outside of the declaration.
            var bodyNode =
                syntaxReference.GetSyntax() switch
                {
                    MethodDeclarationSyntax methodDeclaration => (SyntaxNode?) methodDeclaration.Body ?? methodDeclaration.ExpressionBody,
                    AccessorDeclarationSyntax accessorDeclaration => (SyntaxNode?) accessorDeclaration.Body ?? accessorDeclaration.ExpressionBody,
                    ArrowExpressionClauseSyntax _ => null,
                    PropertyDeclarationSyntax _ => null,
                    EventDeclarationSyntax _ => null,
                    VariableDeclaratorSyntax { Parent: { Parent: EventFieldDeclarationSyntax } } => null,
                    BaseTypeDeclarationSyntax _ => null,
                    LocalFunctionStatementSyntax localFunction => (SyntaxNode?) localFunction.Body ?? localFunction.ExpressionBody,
                    ConstructorDeclarationSyntax constructor => (SyntaxNode?) constructor.Body ?? constructor.ExpressionBody,
                    DestructorDeclarationSyntax destructor => (SyntaxNode?) destructor.Body ?? destructor.ExpressionBody,
                    OperatorDeclarationSyntax @operator => (SyntaxNode?) @operator.Body ?? @operator.ExpressionBody,
                    ConversionOperatorDeclarationSyntax conversionOperator => (SyntaxNode?) conversionOperator.Body ?? conversionOperator.ExpressionBody,
                    _ => throw new AssertionFailedException( $"Don't know how to get the body of a {syntaxReference.GetSyntax().Kind()}" )
                };

            var lookupPosition = bodyNode != null ? bodyNode.Span.Start : syntaxReference.Span.Start;

            foreach ( var definedSymbol in semanticModel.LookupSymbols( lookupPosition ) )
            {
                builder.Add( definedSymbol.Name );
            }

            // Get the symbols defined in the declaration.
            var visitor = new Visitor( builder );
            visitor.Visit( syntaxReference.GetSyntax() );

            return new TemplateLexicalScope( builder.ToImmutable() );
        }
    }
}