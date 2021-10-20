// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Impl.Aspects
{
    internal interface IInheritableAspectsManifest
    {
        IEnumerable<string> InheritableAspectTypes { get; }

        IEnumerable<string> GetInheritableAspectTargets( string aspectType );
    }
}