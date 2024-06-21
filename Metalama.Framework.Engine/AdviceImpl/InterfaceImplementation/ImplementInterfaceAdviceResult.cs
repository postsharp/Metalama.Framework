﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;

internal sealed class ImplementInterfaceAdviceResult : AdviceResult, IImplementInterfaceAdviceResult
{
    private readonly IRef<INamedType>? _target;

    public ImplementInterfaceAdviceResult() { }

    public ImplementInterfaceAdviceResult(
        AdviceOutcome outcome,
        IRef<INamedType>? target,
        ImmutableArray<Diagnostic> diagnostics,
        IReadOnlyCollection<IInterfaceImplementationResult>? interfaces,
        IReadOnlyCollection<IInterfaceMemberImplementationResult>? interfaceMembers )
    {
        this.AdviceKind = AdviceKind.ImplementInterface;
        this.Outcome = outcome;
        this.InterfaceMembers = interfaceMembers ?? Array.Empty<IInterfaceMemberImplementationResult>();
        this.Interfaces = interfaces ?? Array.Empty<IInterfaceImplementationResult>();
        this._target = target;
        this.Diagnostics = diagnostics;
    }

    public IReadOnlyCollection<IInterfaceImplementationResult> Interfaces { get; } = Array.Empty<IInterfaceImplementationResult>();

    public IReadOnlyCollection<IInterfaceMemberImplementationResult> InterfaceMembers { get; } = Array.Empty<IInterfaceMemberImplementationResult>();

    public IInterfaceImplementationAdviser ExplicitMembers
        => this.Interfaces.FirstOrDefault()?.ExplicitMembers ?? throw new InvalidOperationException( "No interfaces were implemented, so explicit implementation is not possible." );
}