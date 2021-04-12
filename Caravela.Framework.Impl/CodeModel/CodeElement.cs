// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        public virtual ICodeElement? ContainingElement => this.Compilation.Factory.GetCodeElement( this.Symbol.ContainingSymbol );

        [Memo]
        public virtual IAttributeList Attributes =>
            new AttributeList(
                this.Symbol!.GetAttributes()
                    .Select( a => new AttributeLink( a, CodeElementLink.FromSymbol<ICodeElement>( this.Symbol ) ) ),
                this.Compilation );

        public abstract CodeElementKind ElementKind { get; }

        public abstract ISymbol Symbol { get; }

        public virtual CodeElementLink<ICodeElement> ToLink() => CodeElementLink.FromSymbol( this.Symbol );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString( format.ToRoslyn() );

        public Location? LocationForDiagnosticReport => DiagnosticLocationHelper.GetLocationForDiagnosticReport( this.Symbol );

        public IReadOnlyList<ISymbol> LookupSymbols()
        {
            if ( this.Symbol.DeclaringSyntaxReferences.Length == 0 )
            {
                throw new InvalidOperationException();
            }

            var syntaxReference = this.Symbol.DeclaringSyntaxReferences[0];
            var semanticModel = this.Compilation.RoslynCompilation.GetSemanticModel( syntaxReference.SyntaxTree );
            var methodBodyNode = ((MethodDeclarationSyntax) syntaxReference.GetSyntax()).Body;
            var lookupPosition = methodBodyNode != null ? methodBodyNode.Span.Start : syntaxReference.Span.Start;

            return semanticModel.LookupSymbols( lookupPosition );
        }
        IDiagnosticLocation? IDiagnosticScope.LocationForDiagnosticReport => this.LocationForDiagnosticReport?.ToDiagnosticLocation();
    }
}
