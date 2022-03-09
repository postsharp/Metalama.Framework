// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.Invokers
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

        protected internal static INamedTypeSymbol? GetTargetTypeSymbol()
        {
            return TemplateExpansionContext.CurrentTargetDeclaration switch
            {
                INamedType type => type.GetSymbol().OriginalDefinition,
                IMember member => member.DeclaringType.GetSymbol().OriginalDefinition,
                null => null,
                _ => throw new AssertionFailedException(),
            };
        }
    }
}