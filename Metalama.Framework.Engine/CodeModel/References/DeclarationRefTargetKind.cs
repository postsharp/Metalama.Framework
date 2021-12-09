// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal enum DeclarationRefTargetKind
    {
        Default,
        Return,
        Assembly,
        Module,
        Field,
        Parameter,
        Method,
        Property,
        Event
    }
}