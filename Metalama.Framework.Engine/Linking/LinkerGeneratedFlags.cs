// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Impl.Linking
{
    [Flags]
    internal enum LinkerGeneratedFlags
    {
        None = 0,
        FlattenableBlock = 1,
        EmptyLabeledStatement = 2
    }
}