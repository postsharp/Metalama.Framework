// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Declaration : IDeclarationImpl
    {
        protected Declaration( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

        ICompilation ICompilationElement.Compilation => this.Compilation;

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        [Memo]
        public virtual IDeclaration? ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this.Symbol.ContainingSymbol );

        [Memo]
        public virtual IAttributeList Attributes
            => new AttributeList(
                this,
                this.Symbol.GetAttributes()
                    .Where( a => a.AttributeConstructor != null )
                    .Select( a => new AttributeRef( a, Ref.FromSymbol<IDeclaration>( this.Symbol, this.Compilation.RoslynCompilation ) ) ) );

        [Memo]
        public IAssembly DeclaringAssembly => this.Compilation.Factory.GetAssembly( this.Symbol.ContainingAssembly );

        public abstract DeclarationKind DeclarationKind { get; }

        public abstract ISymbol Symbol { get; }

        public virtual Ref<IDeclaration> ToRef() => Ref.FromSymbol( this.Symbol, this.Compilation.RoslynCompilation );

        IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

        public virtual string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Symbol.ToDisplayString( format.ToRoslyn() );

        public Location? DiagnosticLocation => this.Symbol.GetDiagnosticLocation();

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

        ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences => this.Symbol.DeclaringSyntaxReferences;

        public abstract bool CanBeInherited { get; }

        public abstract IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true );

        [Memo]
        public IDeclaration OriginalDefinition => this.Compilation.Factory.GetDeclaration( this.Symbol.OriginalDefinition );

        public override string ToString() => this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );
    }
}