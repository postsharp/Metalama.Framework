// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    public interface IEventBuilder : IMemberBuilder, IEvent
    {
        new INamedType EventType { get; set; } // TODO: This should be IType

        new IMethodBuilder Adder { get; }

        new IMethodBuilder Remover { get; }

        new IMethodBuilder? Raiser { get; }
    }
}