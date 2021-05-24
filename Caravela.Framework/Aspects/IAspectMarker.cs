// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Aspects
{
    public interface IAspectMarker { }

    public interface IAspectMarker<in TMarkedDeclaration, TAspectTarget, TAspectClass> : IAspectMarker, IEligible<TMarkedDeclaration>
        where TMarkedDeclaration : class, IDeclaration
        where TAspectTarget : class, IAspectTarget
        where TAspectClass : IAspect<TAspectTarget>, new() { }
}