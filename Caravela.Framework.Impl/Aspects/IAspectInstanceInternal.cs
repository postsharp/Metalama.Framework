// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Aspects
{
    internal interface IAspectInstanceInternal : IAspectInstance, IAspectPredecessorImpl
    {
        void Skip();

        ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }
    }
}