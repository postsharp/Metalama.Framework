// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal interface ISubstitutedDeclaration : IDeclaration
{
    GenericMap GenericMap { get; }
}