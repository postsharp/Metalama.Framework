// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Transformations;

internal interface IMemberLevelTransformation : ITransformation
{
    /// <summary>
    /// Gets a target method base of this code transformation.
    /// </summary>
    IMember TargetMember { get; }
}