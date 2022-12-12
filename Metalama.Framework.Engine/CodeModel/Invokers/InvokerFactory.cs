// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Invokers;
using System;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal sealed class InvokerFactory<T> : IInvokerFactory<T>
        where T : class, IInvoker
    {
        private readonly Func<InvokerOrder, InvokerOperator, T> _createInvoker;
        private readonly bool _hasBaseInvoker;

        public InvokerFactory( Func<InvokerOrder, InvokerOperator, T> createInvoker, bool hasBaseInvoker = true )
        {
            this._createInvoker = createInvoker;
            this._hasBaseInvoker = hasBaseInvoker;
        }

        public T? Base => this._hasBaseInvoker ? this._createInvoker( InvokerOrder.Base, InvokerOperator.Default ) : null;

        public T? ConditionalBase => this._hasBaseInvoker ? this._createInvoker( InvokerOrder.Base, InvokerOperator.Conditional ) : null;

        public T Final => this._createInvoker( InvokerOrder.Default, InvokerOperator.Default );

        public T ConditionalFinal => this._createInvoker( InvokerOrder.Default, InvokerOperator.Conditional );

        public T? GetInvoker( InvokerOrder order, InvokerOperator @operator )
            => (order, @operator) switch
            {
                (InvokerOrder.Base, InvokerOperator.Conditional) => this.ConditionalBase,
                (InvokerOrder.Base, InvokerOperator.Default) => this.Base,
                (InvokerOrder.Default, InvokerOperator.Conditional) => this.ConditionalFinal,
                (InvokerOrder.Default, InvokerOperator.Default) => this.Final,
                _ => throw new ArgumentOutOfRangeException()
            };

        public override string ToString() => "Invokers";
    }
}