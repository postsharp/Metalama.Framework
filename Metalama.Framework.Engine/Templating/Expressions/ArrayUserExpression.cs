// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal class ArrayUserExpression : IUserExpression
    {
        private readonly ArrayBuilder _arrayBuilder;
        private readonly IType _itemType;

        public ArrayUserExpression( ArrayBuilder arrayBuilder )
        {
            this._arrayBuilder = arrayBuilder;

            this._itemType = this._arrayBuilder.ItemType;
            this.Type = this._itemType.ConstructArrayType();
        }

        public ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            var items = this._arrayBuilder.Items.Select( i => RunTimeTemplateExpression.FromValue( i, this.Type.Compilation, syntaxGenerationContext ).Syntax )
                .ToArray();

            var generator = syntaxGenerationContext.SyntaxGenerator;

            return generator.ArrayCreationExpression(
                generator.Type( this._itemType.GetSymbol() ),
                items );
        }

        public RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
        {
            return new RunTimeTemplateExpression(
                this.ToSyntax( TemplateExpansionContext.CurrentSyntaxGenerationContext ),
                this.Type,
                TemplateExpansionContext.CurrentSyntaxGenerationContext );
        }

        public bool IsAssignable => false;

        public IType Type { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}