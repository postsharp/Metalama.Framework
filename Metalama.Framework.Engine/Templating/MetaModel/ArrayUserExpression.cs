// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Impl.CodeModel;
using System;
using System.Linq;

namespace Metalama.Framework.Impl.Templating.MetaModel
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

        public RuntimeExpression ToRunTimeExpression()
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var items = this._arrayBuilder.Items.Select( i => RuntimeExpression.FromValue( i, this.Type.Compilation, syntaxGenerationContext ).Syntax )
                .ToArray();

            var generator = syntaxGenerationContext.SyntaxGenerator;

            var syntax = generator.ArrayCreationExpression(
                generator.Type( this._itemType.GetSymbol() ),
                items );

            return new RuntimeExpression( syntax, this.Type, syntaxGenerationContext );
        }

        public bool IsAssignable => false;

        public IType Type { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}