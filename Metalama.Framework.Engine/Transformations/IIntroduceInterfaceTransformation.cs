// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Transformations;

internal interface IIntroduceInterfaceTransformation : ITransformation
{
    IRef<INamedType> InterfaceType { get; }

    IRef<INamedType> TargetType { get; }
}