// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IReplacedMember : IDeclarationImpl
{
    /// <summary>
    /// Gets a member that is replaced by this transformation or <c>null</c> if the transformation does not replace any member.
    /// </summary>
    MemberRef<IMember> ReplacedMember { get; }
}