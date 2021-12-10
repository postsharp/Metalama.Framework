// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Aspects
{
    internal interface ITransitiveAspectsManifest
    {
        IEnumerable<string> InheritableAspectTypes { get; }

        IEnumerable<InheritableAspectInstance> GetInheritedAspects( string aspectType );
    }
}