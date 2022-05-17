// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a transformation that optionally replaces a member by itself.
    /// </summary>
    internal interface IReplaceMemberTransformation : IObservableTransformation
    {
        /// <summary>
        /// Gets a member that is replaced by this transformation or <c>null</c> if the transformation does not replace any member.
        /// </summary>
        MemberRef<IMember>? ReplacedMember { get; }
    }
}