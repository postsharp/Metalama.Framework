// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class CodeElement : ICodeElementInternal, IHasDiagnosticLocation
    {
        protected CodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        internal CompilationModel Compilation { get; }

        ICompilation ICompilationElement.Compilation => this.Compilation;

        CodeOrigin ICodeElement.Origin => CodeOrigin.Source;

        [Memo]
        public virtual ICodeElement? ContainingElement
            => this.Symbol switch
            {
                IMethodSymbol method when
                    method.MethodKind == MethodKind.PropertyGet
                    || method.MethodKind == MethodKind.PropertySet
                    || method.MethodKind == MethodKind.EventAdd
                    || method.MethodKind == MethodKind.EventRemove
                    || method.MethodKind == MethodKind.EventRaise
                    => this.Compilation.Factory.GetCodeElement( method.AssociatedSymbol.AssertNotNull() ),
                _ => this.Compilation.Factory.GetCodeElement( this.Symbol.ContainingSymbol )
            };

        [Memo]
        public virtual IAttributeList Attributes
            => new AttributeList(
                this,
                this.Symbol!.GetAttributes()
                    .Select( a => new AttributeLink( a, CodeElementLink.FromSymbol<ICodeElement>( this.Symbol ) ) ) );

        public abstract CodeElementKind ElementKind { get; }

        public abstract ISymbol Symbol { get; }

        public virtual CodeElementLink<ICodeElement> ToLink() => CodeElementLink.FromSymbol( this.Symbol );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Symbol.ToDisplayString( format.ToRoslyn() );

        public Location? DiagnosticLocation => this.Symbol.GetDiagnosticLocation();

        public IReadOnlyList<ISymbol> LookupSymbols()
        {
            if ( this.Symbol.DeclaringSyntaxReferences.Length == 0 )
            {
                throw new InvalidOperationException();
            }

            var syntaxReference = this.Symbol.DeclaringSyntaxReferences[0];
            var semanticModel = this.Compilation.RoslynCompilation.GetSemanticModel( syntaxReference.SyntaxTree );

            var bodyNode =
                syntaxReference.GetSyntax() switch
                {
                    MethodDeclarationSyntax methodDeclaration => methodDeclaration.Body,
                    PropertyDeclarationSyntax _ => null,
                    _ => throw new AssertionFailedException()
                };

            var lookupPosition = bodyNode != null ? bodyNode.Span.Start : syntaxReference.Span.Start;

            return semanticModel.LookupSymbols( lookupPosition );
        }

        IDiagnosticLocation? IDiagnosticScope.DiagnosticLocation => this.DiagnosticLocation?.ToDiagnosticLocation();

        ImmutableArray<SyntaxReference> ICodeElementInternal.DeclaringSyntaxReferences => this.Symbol.DeclaringSyntaxReferences;
    }
}