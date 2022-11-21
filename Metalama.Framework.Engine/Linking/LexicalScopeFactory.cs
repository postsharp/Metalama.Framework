// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LexicalScopeFactory : ITemplateLexicalScopeProvider
    {
        /*
         * Calling the SemanticModel.LookupSymbols method is expensive, so expensive that it can be a hotspot of Metalama.
         *
         *  To save calls to LookupSymbols, we cache the result per type declaration, and we discover the rest incrementally from the syntax.
         */

        private readonly ConcurrentDictionary<IDeclaration, TemplateLexicalScope> _scopes;
        private readonly ConcurrentDictionary<TypeDeclarationSyntax, ImmutableHashSet<string>> _identifiersInTypeScope = new();
        private readonly SemanticModelProvider _semanticModelProvider;

        public LexicalScopeFactory( CompilationModel compilation )
        {
            this._semanticModelProvider = compilation.RoslynCompilation.GetSemanticModelProvider();
            this._scopes = new ConcurrentDictionary<IDeclaration, TemplateLexicalScope>( compilation.Comparers.Default );
        }

        /// <summary>
        /// Gets a shared lexical code where consumers can add their own symbols.
        /// </summary>
        public TemplateLexicalScope GetLexicalScope( IDeclaration declaration ) => this._scopes.GetOrAdd( declaration, this.CreateLexicalScope );

        private ImmutableHashSet<string> GetIdentifiersInTypeScope( TypeDeclarationSyntax type )
            => this._identifiersInTypeScope.GetOrAdd( type, this.GetIdentifiersInTypeScopeCore );

        private ImmutableHashSet<string> GetIdentifiersInTypeScopeCore( TypeDeclarationSyntax type )
        {
            var semanticModel = this._semanticModelProvider.GetSemanticModel( type.SyntaxTree );
            var symbols = semanticModel.LookupSymbols( type.OpenBraceToken.Span.End );

            return symbols.Select( s => s.Name ).ToImmutableHashSet();
        }

        private TemplateLexicalScope CreateLexicalScope( IDeclaration declaration )
        {
            var symbol = declaration.GetSymbol();

            if ( symbol == null )
            {
                // Builder-based source.
                switch ( declaration )
                {
                    case IMethod method:
                        return new TemplateLexicalScope( ImmutableHashSet<string>.Empty.AddRange( method.Parameters.Select( p => p.Name ) ) );

                    default:
                        return new TemplateLexicalScope( ImmutableHashSet<string>.Empty );
                }
            }
            else
            {
                // Symbol-based scope.

                var syntaxReference = symbol.GetPrimarySyntaxReference();

                // For implicitly defined symbols, we need to try harder.
                if ( syntaxReference == null )
                {
                    switch ( symbol )
                    {
                        // For accessors, look at the associated symbol.
                        case IMethodSymbol { AssociatedSymbol: { } associatedSymbol }:
                            syntaxReference = associatedSymbol.GetPrimarySyntaxReference();

                            if ( syntaxReference == null )
                            {
                                throw new AssertionFailedException( $"No syntax for '{associatedSymbol}'." );
                            }

                            break;

                        // Otherwise (e.g. for implicit constructors), take the containing type.
                        case { ContainingType: { } containingType }:
                            syntaxReference = containingType.GetPrimarySyntaxReference();

                            if ( syntaxReference == null )
                            {
                                throw new AssertionFailedException( $"No syntax for '{containingType}'." );
                            }

                            break;

                        default:
                            throw new AssertionFailedException( $"Unexpected symbol '{symbol}'." );
                    }
                }

                var builder = this.GetIdentifiersInTypeScope( syntaxReference.GetSyntax().GetDeclaringType().AssertNotNull() ).ToBuilder();

                // Accessors have implicit "value" parameter.
                if ( symbol is IMethodSymbol { MethodKind: RoslynMethodKind.PropertySet or RoslynMethodKind.EventAdd or RoslynMethodKind.EventRemove } )
                {
                    builder.Add( "value" );
                }

                // Get the symbols defined in the declaration.
                var visitor = new Visitor( builder );

                var declarationSyntax =
                    syntaxReference.GetSyntax() switch
                    {
                        { Parent: AccessorListSyntax { Parent: IndexerDeclarationSyntax { } indexer } } => indexer,
                        { } anything => anything,
                    };

                visitor.Visit( declarationSyntax );

                return new TemplateLexicalScope( builder.ToImmutable() );
            }
        }
    }
}