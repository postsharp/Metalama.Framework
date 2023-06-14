// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Advising;

internal sealed partial class ImplementInterfaceAdvice
{
    public sealed class ImplementationResult : IInterfaceImplementationResult
    {
        internal ImplementationResult(
            INamedType @interface,
            InterfaceImplementationOutcome outcome )
        {
            this.Interface = @interface;
            this.Outcome = outcome;
        }

        public INamedType Interface { get; }

        public InterfaceImplementationOutcome Outcome { get; }
    }
}