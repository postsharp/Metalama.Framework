// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Linking
{
    internal enum IntermediateSymbolSemanticTargetKind
    {
        Self,
        PropertyGet,
        PropertySet,
        EventAdd,
        EventRemove,
        EventRaise,
    }
}