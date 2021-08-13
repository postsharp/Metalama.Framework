// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Code.Syntax;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.RunTime;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdvisedFieldOrProperty<T> : AdvisedMember<T>, IAdvisedFieldOrProperty
        where T : IFieldOrProperty, IDeclarationInternal
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
            get => this.ToSyntax();
            set => throw new NotSupportedException();
        }

        public ISyntax ToSyntax()
        {
            if ( this.Invokers.Base == null )
            {
                throw new InvalidOperationException( "Cannot get or set the base value because there is no base property or field." );
            }

            return this.Invokers.Base.GetValue( this.This );
        }
    }
}