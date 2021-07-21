// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Invokers;
using System;

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    internal class InvokerFactory<T> : IInvokerFactory<T>
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

        public T? BaseConditional => this._hasBaseInvoker ? this._createInvoker( InvokerOrder.Base, InvokerOperator.Conditional ) : null;

        public T Final => this._createInvoker( InvokerOrder.Default, InvokerOperator.Default );

        public T FinalConditional => this._createInvoker( InvokerOrder.Default, InvokerOperator.Conditional );
    }
}