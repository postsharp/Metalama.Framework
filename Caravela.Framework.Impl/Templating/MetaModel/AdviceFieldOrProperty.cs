// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects.AdvisedCode;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceFieldOrProperty<T> : AdviceMember<T>, IAdviceFieldOrProperty
        where T : IFieldOrProperty
    {
        public AdviceFieldOrProperty( T underlying ) : base( underlying ) { }

        public IType Type => this.Underlying.Type;

        public IMethod? Getter => this.Underlying.Getter;

        public IMethod? Setter => this.Underlying.Setter;

        public IInvokerFactory<IFieldOrPropertyInvoker> Invoker => this.Underlying.Invoker;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.Underlying.ToFieldOrPropertyInfo();

        public dynamic Value
        {
            get
            {
                if ( this.Invoker.Base == null )
                {
                    throw new InvalidOperationException( "Cannot get or set the base value because there is no base property or field." );
                }

                return this.Invoker.Base.GetValue( this.This );
            }
            set => throw new NotSupportedException();
        }
    }
}