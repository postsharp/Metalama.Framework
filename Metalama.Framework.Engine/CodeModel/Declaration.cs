// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class Declaration : SymbolBasedDeclaration
    {
        protected Declaration( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        public override CompilationModel Compilation { get; }

        public override DeclarationOrigin Origin => DeclarationOrigin.Source;

        [Memo]
        public override IAttributeList Attributes
            => new AttributeList(
                this,
                this.Symbol.GetAttributes()
                    .Where( a => a.AttributeConstructor != null )
                    .Select( a => new AttributeRef( a, Ref.FromSymbol<IDeclaration>( this.Symbol, this.Compilation.RoslynCompilation ) ) ) );

        [Memo]
        public override IAssembly DeclaringAssembly => this.Compilation.Factory.GetAssembly( this.Symbol.ContainingAssembly );

        public override Ref<IDeclaration> ToRef() => Ref.FromSymbol( this.Symbol, this.Compilation.RoslynCompilation );

        public IReadOnlyList<ISymbol> LookupSymbols()
        {
            var syntaxReference = this.Symbol.GetPrimarySyntaxReference();

            // Event fields have accessors without declaring syntax references.
            if ( syntaxReference == null )
            {
                switch ( this.Symbol )
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

            var semanticModel = this.Compilation.RoslynCompilation.GetSemanticModel( syntaxReference.SyntaxTree );

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
                    _ => throw new AssertionFailedException()
                };

            // Accessors have implicit "value" parameter.
            var implicitSymbols =
                bodyNode != null
                    ? Enumerable.Empty<ISymbol>()
                    : this.Symbol switch
                    {
                        IMethodSymbol { MethodKind: RoslynMethodKind.PropertySet or RoslynMethodKind.EventAdd or RoslynMethodKind.EventRemove } methodSymbol =>
                            methodSymbol.Parameters,
                        _ => Enumerable.Empty<ISymbol>()
                    };

            var lookupPosition = bodyNode != null ? bodyNode.Span.Start : syntaxReference.Span.Start;

            return semanticModel.LookupSymbols( lookupPosition ).AddRange( implicitSymbols );
        }

        [Memo]
        public override IDeclaration OriginalDefinition => this.Compilation.Factory.GetDeclaration( this.Symbol.OriginalDefinition );

        public override string ToString() => this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );

        public TExtension GetExtension<TExtension>()
            where TExtension : IMetric
            => this.Compilation.MetricManager.GetMetric<TExtension>( this );
    }
}