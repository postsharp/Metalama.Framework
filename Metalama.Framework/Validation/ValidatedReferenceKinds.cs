// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Validation
{
    [CompileTimeOnly]
    [Flags]
    public enum ValidatedReferenceKinds
    {
        ImplementsInterface,
        Any
    }
}