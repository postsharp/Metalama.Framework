// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.Utilities;
using System;

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    internal abstract class Invoker : IInvoker
    {
        protected AspectReferenceSpecification AspectReference { get; }

        protected ICompilation Compilation { get; }

        protected Invoker( IDeclaration declaration, InvokerOrder order )
        {
            this.Order = order;
            this.Compilation = declaration.Compilation;

            var linkingOrder = order switch
            {
                InvokerOrder.Base => AspectReferenceOrder.Base,
                InvokerOrder.Default => AspectReferenceOrder.Final,
                _ => throw new ArgumentOutOfRangeException( nameof(order), order, null )
            };

            this.AspectReference = new AspectReferenceSpecification(
                UserCodeExecutionContext.Current.AspectLayerId!.Value,
                linkingOrder );
        }

        public InvokerOrder Order { get; }
    }
}