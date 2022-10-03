// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Linking
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