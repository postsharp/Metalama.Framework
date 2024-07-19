// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Transformations;

internal interface IMemberLevelTransformation : ISyntaxTreeTransformation
{
    /// <summary>
    /// Gets a target method base of this code transformation.
    /// </summary>
    IMember TargetMember { get; }
}