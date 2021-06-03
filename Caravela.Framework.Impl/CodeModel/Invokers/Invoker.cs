// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Linking;

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    internal abstract class Invoker : IInvoker
    {
        protected LinkerAnnotation LinkerAnnotation { get; }

        protected Invoker( IDeclaration declaration, InvokerOrder order )
        {
            this.Order = order;
            
            var linkingOrder = order switch
            {
                InvokerOrder.Base => LinkingOrder.Base,
                InvokerOrder.Default => LinkingOrder.Default
            };

            this.LinkerAnnotation = new LinkerAnnotation(
                declaration.GetCompilationModel().AspectLayerId,
                linkingOrder);
        }

        public InvokerOrder Order { get; }
    }
}