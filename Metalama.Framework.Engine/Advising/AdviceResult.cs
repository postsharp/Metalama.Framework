// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising;

/// <summary>
/// Represents the result of a method of <see cref="IAdviceFactory"/>.
/// </summary>
/// <typeparam name="T">The type of declaration returned by the advice method.</typeparam>
internal sealed class AdviceResult<T> : IIntroductionAdviceResult<T>, IOverrideAdviceResult<T>, IImplementInterfaceAdviceResult, IAddContractAdviceResult<T>,
                                        IAddInitializerAdviceResult, IRemoveAttributesAdviceResult
    where T : class, IDeclaration
{
    private readonly IRef<T> _declaration;
    private readonly CompilationModel _compilation;

    /// <summary>
    /// Gets the declaration created or transformed by the advice method. For introduction advice methods, this is the introduced declaration when a new
    /// declaration is introduced, or the existing declaration when a declaration of the same name and signature already exists. For advice that modify a field,
    /// this is the property that now represents the field.
    /// </summary>
    public T Declaration
        => this.Outcome != AdviceOutcome.Error
            ? this._declaration.GetTarget( this._compilation, ReferenceResolutionOptions.CanBeMissing ).Assert( d => d is not IDeclarationBuilder )
            : throw new InvalidOperationException( "Cannot get the resulting declaration when the outcome is Error." );

    public AdviceKind AdviceKind { get; }

    public AdviceOutcome Outcome { get; }

    public IAspectBuilder AspectBuilder { get; }

    public IReadOnlyCollection<InterfaceImplementationResult> Interfaces { get; }

    public IReadOnlyCollection<InterfaceMemberImplementationResult> InterfaceMembers { get; }

    internal AdviceResult( IRef<T> declaration, CompilationModel compilation, AdviceOutcome outcome, IAspectBuilder aspectBuilder, AdviceKind adviceKind, IReadOnlyCollection<InterfaceImplementationResult> interfaces, IReadOnlyCollection<InterfaceMemberImplementationResult> interfaceMembers )
    {
        this._declaration = declaration;
        this._compilation = compilation.Assert( c => c.IsMutable );
        this.Outcome = outcome;
        this.AspectBuilder = aspectBuilder;
        this.AdviceKind = adviceKind;
        this.Interfaces = interfaces;
        this.InterfaceMembers = interfaceMembers;
    }
}