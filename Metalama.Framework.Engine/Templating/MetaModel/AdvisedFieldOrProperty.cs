// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal class AdvisedFieldOrProperty<T> : AdvisedFieldOrPropertyOrIndexer<T>, IAdvisedFieldOrProperty, IUserExpression
        where T : IFieldOrProperty, IDeclarationImpl
    {
        public AdvisedFieldOrProperty( T underlying ) : base( underlying ) { }

        public bool? IsAutoPropertyOrField => this.Underlying.IsAutoPropertyOrField;

        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers => this.Underlying.Invokers;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.Underlying.ToFieldOrPropertyInfo();

        private IExpression ToExpression()
        {
            if ( this.Invokers.Base == null )
            {
                return this.Invokers.Final.GetValue( this.This );
            }
            else
            {
                return this.Invokers.Base.GetValue( this.This );
            }
        }

        public object? Value
        {
            get => this.ToExpression();
            set => throw new NotSupportedException();
        }

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => new TypedExpressionSyntaxImpl(
                this.ToExpressionSyntax(),
                this.Type,
                syntaxGenerationContext );
    }
}