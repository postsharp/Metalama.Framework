// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    /// <summary>
    /// The base class for the read-only facade of introduced declarations, represented by <see cref="DeclarationBuilder"/>. Facades
    /// are consistent with the consuming <see cref="CompilationModel"/>, while builders are consistent with the producing <see cref="CompilationModel"/>. 
    /// </summary>
    internal abstract class BuiltDeclaration : BaseDeclaration, IRefImpl
    {
        protected BuiltDeclaration( CompilationModel compilation, IDeclarationBuilder builder )
        {
            this.Compilation = compilation;
            _ = builder;
        }

        public override CompilationModel Compilation { get; }

        public abstract DeclarationBuilder Builder { get; }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Builder.ToDisplayString( format, context );

        [Memo]
        public override IAssembly DeclaringAssembly => this.Compilation.Factory.GetDeclaration( this.Builder.DeclaringAssembly );

        public override IDeclarationOrigin Origin => this.Builder.Origin;

        [Memo]
        public override IDeclaration? ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this.Builder.ContainingDeclaration );

        public override SyntaxTree? PrimarySyntaxTree => this.ContainingDeclaration?.GetPrimarySyntaxTree();

        [Memo]
        public override IAttributeCollection Attributes
            => new AttributeCollection(
                this,
                this.GetCompilationModel().GetAttributeCollection( this.ToTypedRef<IDeclaration>() ) );

        public override DeclarationKind DeclarationKind => this.Builder.DeclarationKind;

        protected IDeclaration GetForCompilation( ICompilation compilation, ReferenceResolutionOptions options )
            => this.GetForCompilation( (CompilationModel) compilation, options );

        protected IDeclaration GetForCompilation( CompilationModel compilation, ReferenceResolutionOptions options )
            => this.Compilation == compilation ? this : compilation.Factory.GetDeclaration( this.Builder, options );

        internal override Ref<IDeclaration> ToRef() => Ref.FromBuilder( this.Builder );

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Builder.DeclaringSyntaxReferences;

        public override bool CanBeInherited => this.Builder.CanBeInherited;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override string ToString() => this.Builder.ToString();

        [Memo]
        public override IDeclaration OriginalDefinition => this.Compilation.Factory.GetDeclaration( this.Builder.OriginalDefinition );

        public override Location? DiagnosticLocation => this.Builder.DiagnosticLocation;

        object? IRefImpl.Target => this;

        bool IRefImpl.IsDefault => false;

        public sealed override bool IsImplicitlyDeclared => false;
    }
}