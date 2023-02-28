// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal static class DeclarationRefTargetKindExtensions
{
    public static DeclarationRefTargetKind ToDeclarationRefTargetKind(
        this MethodKind methodKind,
        DeclarationRefTargetKind existingTargetKind = DeclarationRefTargetKind.Default )
        => (methodKind, existingTargetKind) switch
        {
            (MethodKind.PropertyGet, DeclarationRefTargetKind.Default) => DeclarationRefTargetKind.PropertyGet,
            (MethodKind.PropertySet, DeclarationRefTargetKind.Default) => DeclarationRefTargetKind.PropertySet,
            (MethodKind.EventRaise, DeclarationRefTargetKind.Default) => DeclarationRefTargetKind.EventRaise,
            (MethodKind.PropertySet, DeclarationRefTargetKind.Parameter) => DeclarationRefTargetKind.PropertySetParameter,
            (MethodKind.EventRaise, DeclarationRefTargetKind.Parameter) => DeclarationRefTargetKind.EventRaiseParameter,
            (MethodKind.PropertyGet, DeclarationRefTargetKind.Return) => DeclarationRefTargetKind.PropertyGetReturnParameter,
            (MethodKind.PropertySet, DeclarationRefTargetKind.Return) => DeclarationRefTargetKind.PropertySetReturnParameter,
            (MethodKind.EventRaise, DeclarationRefTargetKind.Return) => DeclarationRefTargetKind.EventRaiseReturnParameter,
            _ => throw new InvalidOperationException( $"Unexpected combination: '{methodKind}', '{existingTargetKind}'." )
        };
}