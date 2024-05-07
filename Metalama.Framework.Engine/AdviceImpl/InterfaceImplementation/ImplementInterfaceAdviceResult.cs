// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;

internal class ImplementInterfaceAdviceResult : AdviceResult, IImplementInterfaceAdviceResult
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

    public INamedType Target => this.Resolve( this._target );

    public IAdviser<TNewDeclaration> WithTarget<TNewDeclaration>( TNewDeclaration target )
        where TNewDeclaration : IDeclaration
        => throw new NotImplementedException();

    public IReadOnlyCollection<IInterfaceImplementationResult> Interfaces { get; } = Array.Empty<IInterfaceImplementationResult>();

    public IReadOnlyCollection<IInterfaceMemberImplementationResult> InterfaceMembers { get; } = Array.Empty<IInterfaceMemberImplementationResult>();

    public IAdviser<INamedType> WithExplicitImplementation() => throw new NotImplementedException();
}