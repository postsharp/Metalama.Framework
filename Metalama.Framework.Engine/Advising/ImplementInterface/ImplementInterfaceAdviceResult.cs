// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Advising;

internal class ImplementInterfaceAdviceResult : AdviceResult, IImplementInterfaceAdviceResult
{
    private readonly IReadOnlyCollection<IInterfaceMemberImplementationResult>? _interfaceMembers;
    private readonly IReadOnlyCollection<IInterfaceImplementationResult>? _interfaces;
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
        this._interfaceMembers = interfaceMembers;
        this._interfaces = interfaces;
        this._target = target;
        this.Diagnostics = diagnostics;
    }

    public INamedType Target => this.Resolve( this._target );

    public IAdvisable<TNewDeclaration> WithTarget<TNewDeclaration>( TNewDeclaration target )
        where TNewDeclaration : IDeclaration
        => throw new NotImplementedException();

    public IReadOnlyCollection<IInterfaceImplementationResult> Interfaces => this._interfaces ?? throw new InvalidOperationException();

    public IReadOnlyCollection<IInterfaceMemberImplementationResult> InterfaceMembers => this._interfaceMembers ?? throw new InvalidOperationException();

    public IAdvisable<INamedType> WithExplicitImplementation() => throw new NotImplementedException();
}