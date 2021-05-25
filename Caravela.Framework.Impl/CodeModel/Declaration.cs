// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Declaration : IDeclarationInternal, IHasDiagnosticLocation
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
                this.Symbol!.GetAttributes()
                    .Select( a => new AttributeRef( a, DeclarationRef.FromSymbol<IDeclaration>( this.Symbol ) ) ) );

        public abstract DeclarationKind DeclarationKind { get; }

        public bool HasAspect<T>()
            where T : IAspect
            => throw new NotImplementedException();

        [Obsolete( "Not implemented." )]
        public IAnnotationList GetAnnotations<T>()
            where T : IAspect
            => throw new NotImplementedException();

        public abstract ISymbol Symbol { get; }

        public virtual DeclarationRef<IDeclaration> ToRef() => DeclarationRef.FromSymbol( this.Symbol );

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

        ImmutableArray<SyntaxReference> IDeclarationInternal.DeclaringSyntaxReferences => this.Symbol.DeclaringSyntaxReferences;
    }
}