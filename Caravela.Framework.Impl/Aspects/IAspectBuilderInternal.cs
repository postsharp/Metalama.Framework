// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Advices;

namespace Caravela.Framework.Impl.Aspects
{
    internal interface IAspectBuilderInternal : IAspectBuilder
    {
        void AddAspectSource( IAspectSource aspectSource );
        AdviceFactory AdviceFactory { get; }
    }
}