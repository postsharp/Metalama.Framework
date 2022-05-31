// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal class AdvisedFieldOrProperty<T> : AdvisedMember<T>, IAdvisedFieldOrProperty, IUserExpression
        where T : IFieldOrProperty, IDeclarationImpl
    {
        public AdvisedFieldOrProperty( T underlying ) : base( underlying ) { }

        public IType Type => this.Underlying.Type;

        public bool IsAssignable => this.Underlying.Writeability >= Writeability.ConstructorOnly;

        public IMethod? GetMethod => this.Underlying.GetMethod;

        public IMethod? SetMethod => this.Underlying.SetMethod;

        public Writeability Writeability => this.Underlying.Writeability;

        public bool IsAutoPropertyOrField => this.Underlying.IsAutoPropertyOrField;

        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers => this.Underlying.Invokers;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.Underlying.ToFieldOrPropertyInfo();

        public object? Value
        {
            get => this.ToExpression();
            set => throw new NotSupportedException();
        }

        private IExpression ToExpression()
        {
            if ( this.Invokers.Base == null )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format( $"Cannot get or set the base value of '{this}' because there is no base property or field." ) );
            }

            return this.Invokers.Base.GetValue( this.This );
        }

        public IMethod? GetAccessor( MethodKind methodKind ) => this.Underlying.GetAccessor( methodKind );

        public IEnumerable<IMethod> Accessors => this.Underlying.Accessors;

        public ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => SyntaxFactory.IdentifierName( this.Underlying.Name );

        public RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
            => new(
                this.ToSyntax( syntaxGenerationContext ),
                this.Type,
                syntaxGenerationContext );
    }
}