// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Impl.Linking
{
    [Flags]
    internal enum LinkerDeclarationFlags
    {
        None = 0,

        /// <summary>
        /// Used to denote event field declaration where event field declaration is not possible (e.g. explicit interface implementation with event field template).
        /// </summary>
        EventField = 1,

        /// <summary>
        /// Used to denote a declaration body of which should not be inlined by the linker.
        /// </summary>
        NotInlineable = 1 << 15,

        /// <summary>
        /// User to denote a declaration which should not be discarded. 
        /// </summary>
        NotDiscardable = 1 << 16
    }
}