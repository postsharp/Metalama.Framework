// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Linking
{
    [Flags]
    internal enum LinkerDeclarationFlags
    {
        None = 0,
        EventField = 1
    }
}