﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        protected virtual void OnUsingDeclaration() { }

        public override CompilationModel Compilation { get; }

        public override IAttributeCollection Attributes
        {
            get
            {
                this.OnUsingDeclaration();

                return this.AttributesImpl;
            }
        }

        [Memo]
        private IAttributeCollection AttributesImpl
            => new AttributeCollection(
                this,
                this.Compilation.GetAttributeCollection( this.ToTypedRef<IDeclaration>() ) );

        [Memo]
        public override IAssembly DeclaringAssembly => this.Compilation.Factory.GetAssembly( this.Symbol.ContainingAssembly );

        internal override Ref<IDeclaration> ToRef()
        {
            this.OnUsingDeclaration();

            return Ref.FromSymbol( this.Symbol, this.Compilation.RoslynCompilation );
        }

        [Memo]
        public override IDeclaration OriginalDefinition => this.Compilation.Factory.GetDeclaration( this.Symbol.OriginalDefinition );

        public override string ToString()
        {
            this.OnUsingDeclaration();

            return this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );
        }

        public TExtension GetExtension<TExtension>()
            where TExtension : IMetric
            => this.Compilation.MetricManager.GetMetric<TExtension>( this );
    }
}