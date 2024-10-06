// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents a transformation that optionally replaces a member by itself.
/// </summary>
internal interface IReplaceMemberTransformation : ITransformation
{
    /// <summary>
    /// Gets a member that is replaced by this transformation or <c>null</c> if the transformation does not replace any member.
    /// </summary>
    IRef<IMember>? ReplacedMember { get; }

    // ReplacedMember must not be a reference because resolving the reference would returned the replacement, not the original member.
}