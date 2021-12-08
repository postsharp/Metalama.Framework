// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using static Metalama.Framework.Diagnostics.Severity;

// ReSharper disable SA1118

namespace Metalama.Framework.Impl.Linking
{
    internal static class AspectLinkerDiagnosticDescriptors
    {
        // Reserved range 600-599

        private const string _category = "Metalama.Linker";

        public static readonly DiagnosticDefinition<(string AspectType, ISymbol TargetDeclaration)>
            CannotUseBaseInvokerWithNonInstanceExpression = new(
                "CR0600",
                "Cannot use Base invoker with non-this instance expression.",
                "The aspect '{0}' on '{1}' uses Base invoker with an instance expression different than 'this'."
                + " Use 'meta.This' as the first argument or use Final invoker.",
                _category,
                Error );
    }
}