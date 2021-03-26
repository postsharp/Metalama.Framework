// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    /// <summary>
    /// The base class for the read-only facade of introduced code elements, represented by <see cref="CodeElementBuilder"/>. Facades
    /// are consistent with the consuming <see cref="CompilationModel"/>, while builders are consistent with the producing <see cref="CompilationModel"/>. 
    /// </summary>
    internal abstract class BuiltCodeElement : ICodeElementInternal
    {
        protected BuiltCodeElement( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        public CompilationModel Compilation { get; }

        public abstract CodeElementBuilder Builder { get; }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Builder.ToDisplayString( format, context );

        public IDiagnosticLocation? DiagnosticLocation => this.Builder.DiagnosticLocation;

        CodeOrigin ICodeElement.Origin => CodeOrigin.Aspect;

        public ICodeElement? ContainingElement => this.Compilation.Factory.GetCodeElement( this.Builder );

        [Memo]
        public IAttributeList Attributes =>
            new AttributeList(
                this.Builder.Attributes
                    .Select<AttributeBuilder, AttributeLink>( a => new AttributeLink( a ) ),
                this.Compilation );

        public CodeElementKind ElementKind => this.Builder.ElementKind;

        ICompilation ICodeElement.Compilation => this.Compilation;

        protected ICodeElement GetForCompilation( CompilationModel compilation )
            => this.Compilation == compilation ? this : compilation.Factory.GetCodeElement( this.Builder );

        ISymbol? ISdkCodeElement.Symbol => null;

        public CodeElementLink<ICodeElement> ToLink() => CodeElementLink.FromBuilder( this.Builder );
    }
}