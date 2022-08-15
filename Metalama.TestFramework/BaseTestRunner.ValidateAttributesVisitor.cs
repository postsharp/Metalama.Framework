// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Metalama.TestFramework
{
    public abstract partial class BaseTestRunner
    {
        private class ValidateAttributesVisitor : SafeSyntaxWalker
        {
            private readonly Compilation _compilation;

            public ValidateAttributesVisitor( Compilation compilation )
            {
                this._compilation = compilation;
            }

            public override void VisitAttribute( AttributeSyntax node )
            {
                var semanticModel = this._compilation.GetSemanticModel( node.SyntaxTree );
                var symbol = semanticModel.GetSymbolInfo( node.Name );

                if ( symbol.Symbol == null )
                {
                    Assert.True(
                        false,
                        $"The custom attribute '{node}' cannot be resolved. Check that you are importing the correct namespaces and assemblies." );
                }
            }
        }
    }
}