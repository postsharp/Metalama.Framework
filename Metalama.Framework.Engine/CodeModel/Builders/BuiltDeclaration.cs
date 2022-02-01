// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    /// <summary>
    /// The base class for the read-only facade of introduced declarations, represented by <see cref="DeclarationBuilder"/>. Facades
    /// are consistent with the consuming <see cref="CompilationModel"/>, while builders are consistent with the producing <see cref="CompilationModel"/>. 
    /// </summary>
    internal abstract class BuiltDeclaration : BaseDeclaration
    {
        protected BuiltDeclaration( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        public override CompilationModel Compilation { get; }

        public abstract DeclarationBuilder Builder { get; }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Builder.ToDisplayString( format, context );

        public override IAssembly DeclaringAssembly => this.Builder.DeclaringAssembly;

        public override DeclarationOrigin Origin => DeclarationOrigin.Aspect;

        public override IDeclaration? ContainingDeclaration => this.Builder.ContainingDeclaration;

        [Memo]
        public override IAttributeList Attributes
            => new AttributeList(
                this,
                this.Builder.Attributes
                    .Select<AttributeBuilder, AttributeRef>( a => new AttributeRef( a ) ) );

        public override DeclarationKind DeclarationKind => this.Builder.DeclarationKind;

        protected IDeclaration GetForCompilation( ICompilation compilation ) => this.GetForCompilation( (CompilationModel) compilation );

        protected IDeclaration GetForCompilation( CompilationModel compilation )
            => this.Compilation == compilation ? this : compilation.Factory.GetDeclaration( this.Builder );

        internal override Ref<IDeclaration> ToRef() => Ref.FromBuilder( this.Builder );

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Builder.DeclaringSyntaxReferences;

        public override bool CanBeInherited => this.Builder.CanBeInherited;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override string ToString() => this.Builder.ToString();

        [Memo]
        public override IDeclaration OriginalDefinition => this.Compilation.Factory.GetDeclaration( this.Builder.OriginalDefinition );

        public override Location? DiagnosticLocation => this.Builder.DiagnosticLocation;
    }
}