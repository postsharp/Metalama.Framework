// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class UserExpression : IDynamicExpression
    {
        public IDynamicExpression Underlying { get; }

        public UserExpression( RuntimeExpression? underlying, ICompilation compilation )
        {
            if ( underlying == null )
            {
                this.Underlying = new DefaultDynamicExpression( compilation.TypeFactory.GetSpecialType( SpecialType.Object ) );
            }
            else
            {
                var type = underlying.ExpressionType != null
                    ? compilation.GetCompilationModel().Factory.GetIType( underlying.ExpressionType )
                    : compilation.TypeFactory.GetSpecialType( SpecialType.Object ).MakeNullable();

                this.Underlying = new DynamicExpression( underlying.Syntax, type );
            }
        }

        public IType Type => this.Underlying.Type;

        public bool IsAssignable => this.Underlying.IsAssignable;

        public object? Value
        {
            get => this.ToExpression();
            set => throw new NotSupportedException();
        }

        RuntimeExpression IDynamicExpression.CreateExpression( string? expressionText, Location? location )
            => this.Underlying.CreateExpression( expressionText, location );

        private IExpression ToExpression() => this.Underlying;
    }
}