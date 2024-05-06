// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;

internal sealed partial class ImplementInterfaceAdvice
{
    public sealed record ImplementationResult( INamedType InterfaceType, InterfaceImplementationOutcome Outcome ) : IInterfaceImplementationResult;
}