// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.RunTime
{
    // NOTE: There seems to be no reasonable way to reference the aspect layer in these types (without generating a type for each layer), so overrides
    //       on different layers will counted through ordinals. This may be quite confusing, since there may be multiple overrides within the same
    //       layer, but should be rare and limited to non-renameable declarations with specific semantics (indexers and constructors).

    /// <summary>
    /// A type of a superficial parameter used by Metalama to denote declaration overrides.
    /// </summary>
    /// <typeparam name="TAspectType">Type of the aspect that overrides the declaration.</typeparam>
    public readonly struct OverriddenBy<TAspectType>
        where TAspectType : IAspect
    {
        // Empty structs always have 1 byte.
    }

    /// <summary>
    /// A type of a superficial parameter used by Metalama to denote declaration overrides.
    /// </summary>
    /// <typeparam name="TAspectType">Type of the aspect that overrides the declaration.</typeparam>
    /// <typeparam name="TOrdinal">Ordinal of the override by the aspect if it overrides the declaration more than once (e.g. in two layers).</typeparam>
    public readonly struct OverriddenBy<TAspectType, TOrdinal>
        where TAspectType : IAspect
        where TOrdinal : struct, IOverridenByOrdinal
    {
        // Empty structs always have 1 byte.
    }
}
