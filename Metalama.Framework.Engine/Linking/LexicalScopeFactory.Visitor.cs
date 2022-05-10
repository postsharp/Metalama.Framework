// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LexicalScopeFactory
{
    private class Visitor : CSharpSyntaxWalker
    {
        private readonly ImmutableHashSet<string>.Builder _builder;

        public Visitor( ImmutableHashSet<string>.Builder builder )
        {
            this._builder = builder;
        }

        public override void VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
        {
            this._builder.Add( node.Identifier.Text );

            base.VisitLocalFunctionStatement( node );
        }

        public override void VisitVariableDeclarator( VariableDeclaratorSyntax node )
        {
            this._builder.Add( node.Identifier.Text );
        }

        public override void VisitParameter( ParameterSyntax node )
        {
            this._builder.Add( node.Identifier.Text );
        }

        public override void VisitSingleVariableDesignation( SingleVariableDesignationSyntax node ) => this._builder.Add( node.Identifier.Text );
    }
}