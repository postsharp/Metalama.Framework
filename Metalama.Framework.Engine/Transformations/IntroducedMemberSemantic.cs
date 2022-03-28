// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.Transformations
{
    internal enum IntroducedMemberSemantic
    {
        Introduction,
        Override,
        GetterOverride,
        SetterOverride,
        EventOverride,
        AdderOverride,
        RemoverOverride,
        RaiserOverride,
        InterfaceImplementation,
        InitializerMethod,
        Initialization
    }
}