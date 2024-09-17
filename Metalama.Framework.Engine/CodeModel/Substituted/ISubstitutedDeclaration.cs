// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

/// <summary>
/// Represents a generic instance of a type or member.
/// </summary>
internal interface ISubstitutedDeclaration : IDeclaration
{
    IDeclaration Definition { get; }
}