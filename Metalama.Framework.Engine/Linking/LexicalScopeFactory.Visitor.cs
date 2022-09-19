// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LexicalScopeFactory
{
    private class Visitor : SafeSyntaxWalker
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

        public override void VisitVariableDeclarator( VariableDeclaratorSyntax node ) => this._builder.Add( node.Identifier.Text );

        public override void VisitParameter( ParameterSyntax node ) => this._builder.Add( node.Identifier.Text );

        public override void VisitTypeParameter( TypeParameterSyntax node ) => this._builder.Add( node.Identifier.Text );

        public override void VisitSingleVariableDesignation( SingleVariableDesignationSyntax node ) => this._builder.Add( node.Identifier.Text );
    }
}