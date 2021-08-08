// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class ArrayDynamicExpression : IDynamicExpression
    {
        private readonly ArrayBuilder _arrayBuilder;
        private readonly IType _itemType;

        public ArrayDynamicExpression( ArrayBuilder arrayBuilder, ICompilation compilation )
        {
            this._arrayBuilder = arrayBuilder;

            this._itemType = this._arrayBuilder.ItemType switch
            {
                IType ourType => ourType,
                Type reflectionType => compilation.TypeFactory.GetTypeByReflectionType( reflectionType ),
                _ => throw new AssertionFailedException()
            };

            this.ExpressionType = this._itemType.MakeArrayType();
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null )
        {
            var items = this._arrayBuilder.Items.Select( i => RuntimeExpression.FromValue( i, this.ExpressionType.Compilation ).Syntax ).ToArray();

            var generator = LanguageServiceFactory.CSharpSyntaxGenerator;

            var arrayCreation = generator.ArrayCreationExpression(
                generator.TypeExpression( this._itemType.GetSymbol() ),
                items );

            return new RuntimeExpression( arrayCreation, this.ExpressionType );
        }

        public bool IsAssignable => false;

        public IType ExpressionType { get; }
    }
}