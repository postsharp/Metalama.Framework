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

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal abstract class AdvisedFieldOrProperty<T> : AdvisedFieldOrPropertyOrIndexer<T>, IAdvisedFieldOrProperty, IUserExpression
        where T : IFieldOrProperty, IDeclarationImpl
    {
        protected AdvisedFieldOrProperty( T underlying ) : base( underlying ) { }

        public bool? IsAutoPropertyOrField => this.Underlying.IsAutoPropertyOrField;

        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers => this.Underlying.Invokers;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.Underlying.ToFieldOrPropertyInfo();

        public bool IsRequired => this.Underlying.IsRequired;

        public IExpression? InitializerExpression => this.Underlying.InitializerExpression;

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

        public ref object? Value => ref RefHelper.Wrap( this.ToExpression() );

        private ExpressionSyntax ToExpressionSyntax() => SyntaxFactory.IdentifierName( this.Underlying.Name );

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => new TypedExpressionSyntaxImpl(
                this.ToExpressionSyntax(),
                this.Type,
                syntaxGenerationContext );
    }
}