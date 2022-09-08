// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class Declaration : SymbolBasedDeclaration
    {
        protected Declaration( CompilationModel compilation, ISymbol symbol ) : base( symbol )
        {
            this.Compilation = compilation;
        }

        public override CompilationModel Compilation { get; }

        public override DeclarationOrigin Origin => DeclarationOrigin.Source;

        [Memo]
        public override IAttributeCollection Attributes
            => new AttributeCollection(
                this,
                this.Compilation.GetAttributeCollection( this.ToTypedRef<IDeclaration>() ) );

        [Memo]
        public override IAssembly DeclaringAssembly => this.Compilation.Factory.GetAssembly( this.Symbol.ContainingAssembly );

        internal override Ref<IDeclaration> ToRef() => Ref.FromSymbol( this.Symbol, this.Compilation.RoslynCompilation );

        [Memo]
        public override IDeclaration OriginalDefinition => this.Compilation.Factory.GetDeclaration( this.Symbol.OriginalDefinition );

        public override string ToString() => this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );

        public TExtension GetExtension<TExtension>()
            where TExtension : IMetric
            => this.Compilation.MetricManager.GetMetric<TExtension>( this );
    }
}