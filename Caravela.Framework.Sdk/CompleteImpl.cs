// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Sdk
{

    public abstract partial class PartialCompilation
    {
        private class CompleteImpl : PartialCompilation
        {
            public CompleteImpl( Compilation compilation ) : base( compilation ) { }

            public override IEnumerable<SyntaxTree> SyntaxTrees => this.Compilation.SyntaxTrees;

            public override IEnumerable<ITypeSymbol> Types => this.Compilation.Assembly.GetTypes();

            public override bool IsPartial { get; }

            public override PartialCompilation ReplaceSyntaxTree( SyntaxTree oldTree, SyntaxTree newTree )
                => new CompleteImpl( this.Compilation.ReplaceSyntaxTree( oldTree, newTree ) );
        }
    }
}