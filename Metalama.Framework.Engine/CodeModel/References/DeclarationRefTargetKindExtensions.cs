// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal static class DeclarationRefTargetKindExtensions
{
    public static RefTargetKind ToDeclarationRefTargetKind(
        this MethodKind methodKind,
        RefTargetKind existingTargetKind = RefTargetKind.Default )
        => (methodKind, existingTargetKind) switch
        {
            (MethodKind.PropertyGet, RefTargetKind.Default) => RefTargetKind.PropertyGet,
            (MethodKind.PropertySet, RefTargetKind.Default) => RefTargetKind.PropertySet,
            (MethodKind.EventRaise, RefTargetKind.Default) => RefTargetKind.EventRaise,
            (MethodKind.PropertySet, RefTargetKind.Parameter) => RefTargetKind.PropertySetParameter,
            (MethodKind.EventRaise, RefTargetKind.Parameter) => RefTargetKind.EventRaiseParameter,
            (MethodKind.PropertyGet, RefTargetKind.Return) => RefTargetKind.PropertyGetReturnParameter,
            (MethodKind.PropertySet, RefTargetKind.Return) => RefTargetKind.PropertySetReturnParameter,
            (MethodKind.EventRaise, RefTargetKind.Return) => RefTargetKind.EventRaiseReturnParameter,
            _ => throw new InvalidOperationException( $"Unexpected combination: '{methodKind}', '{existingTargetKind}'." )
        };
}