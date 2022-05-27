﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System.Linq;

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
        public override IAttributeCollection Attributes
            => new AttributeCollection(
                this,
                this.Symbol.GetAttributes()
                    .Where( a => a.AttributeConstructor != null )
                    .Select( a => new AttributeRef( a, Ref.FromSymbol<IDeclaration>( this.Symbol, this.Compilation.RoslynCompilation ) ) )
                    .ToList() );

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