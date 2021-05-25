// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAspectMarkerInstance
    {
        IAspectMarker Marker { get; }

        IDeclaration MarkedDeclaration { get; }
    }
}