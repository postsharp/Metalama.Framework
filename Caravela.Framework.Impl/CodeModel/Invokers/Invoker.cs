// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using System;

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    internal abstract class Invoker : IInvoker
    {
        protected AspectReferenceSpecification LinkerAnnotation { get; }

        protected Invoker( IDeclaration declaration, InvokerOrder order )
        {
            this.Order = order;

            var linkingOrder = order switch
            {
                InvokerOrder.Base => AspectReferenceOrder.Base,
                InvokerOrder.Default => AspectReferenceOrder.Default,
                _ => throw new ArgumentOutOfRangeException( nameof(order), order, null )
            };

            this.LinkerAnnotation = new AspectReferenceSpecification(
                declaration.GetCompilationModel().AspectLayerId,
                linkingOrder );
        }

        public InvokerOrder Order { get; }
    }
}