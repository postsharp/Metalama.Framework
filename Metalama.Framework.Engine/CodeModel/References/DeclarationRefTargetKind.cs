// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;

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

    internal static class DeclarationRefTargetKindExtensions
    {
        public static DeclarationRefTargetKind ToDeclarationRefTargetKind( this MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => DeclarationRefTargetKind.PropertyGet,
                MethodKind.PropertySet => DeclarationRefTargetKind.PropertySet,
                MethodKind.EventRaise => DeclarationRefTargetKind.EventRaise,
                _ => throw new ArgumentOutOfRangeException( $"Unexpected value: '{methodKind}'." )
            };
    }
}