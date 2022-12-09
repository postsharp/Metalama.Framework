// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal enum DeclarationRefTargetKind
    {
        // WARNING! These values are long-term serialized. Do not rename.

        Default,
        Return,
        Assembly,
        Module,
        Field,
        Parameter,
        Method,
        Property,
        Event,
        PropertyGet,
        PropertySet,
        StaticConstructor,
        Finalizer,
        PropertySetParameter,
        PropertyGetReturnParameter,
        PropertySetReturnParameter,
        EventRaise,
        EventRaiseParameter,
        EventRaiseReturnParameter
    }
}