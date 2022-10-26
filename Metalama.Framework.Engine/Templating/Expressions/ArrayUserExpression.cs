// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal class ArrayUserExpression : UserExpression
    {
        private readonly ArrayBuilder _arrayBuilder;
        private readonly IType _itemType;

        public ArrayUserExpression( ArrayBuilder arrayBuilder )
        {
            this._arrayBuilder = arrayBuilder;

            this._itemType = this._arrayBuilder.ItemType;
            this.Type = this._itemType.MakeArrayType();
        }

        protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            var items = this._arrayBuilder.Items.Select( i => TypedExpressionSyntax.FromValue( i, this.Type.Compilation, syntaxGenerationContext ).Syntax )
                .ToArray();

            var generator = syntaxGenerationContext.SyntaxGenerator;

            return generator.ArrayCreationExpression(
                generator.Type( this._itemType.GetSymbol() ),
                items );
        }

        public override IType Type { get; }
    }
}