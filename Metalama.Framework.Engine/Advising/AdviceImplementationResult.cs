// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class AdviceImplementationResult
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        // This property is used only by the introspection API.
        public ImmutableArray<ITransformation> Transformations { get; internal set; } = ImmutableArray<ITransformation>.Empty;

        public AdviceOutcome Outcome { get; }

        public Ref<IDeclaration> NewDeclaration { get; }

        // These properties are used only for introduce interface advice.
        public IReadOnlyCollection<InterfaceImplementationResult> Interfaces { get; }

        public IReadOnlyCollection<InterfaceMemberImplementationResult> InterfaceMembers { get; }

        private AdviceImplementationResult( AdviceOutcome outcome, in Ref<IDeclaration> newDeclaration, ImmutableArray<Diagnostic> diagnostics, IReadOnlyCollection<InterfaceImplementationResult> interfaces, IReadOnlyCollection<InterfaceMemberImplementationResult> interfaceMembers )
        {
            this.Diagnostics = diagnostics;
            this.Outcome = outcome;
            this.NewDeclaration = newDeclaration;
            this.Interfaces = interfaces;
            this.InterfaceMembers = interfaceMembers;
        }

        public static AdviceImplementationResult Success( IDeclaration newDeclaration ) => Success( AdviceOutcome.Default, newDeclaration.ToTypedRef() );

        public static AdviceImplementationResult Success(
            AdviceOutcome outcome = AdviceOutcome.Default,
            Ref<IDeclaration> newDeclaration = default,
            ImmutableArray<Diagnostic>? diagnostics = null,
            IReadOnlyCollection<InterfaceImplementationResult>? interfaces = null,
            IReadOnlyCollection<InterfaceMemberImplementationResult>? interfaceMembers = null )
            => new(
                outcome,
                newDeclaration,
                diagnostics ?? ImmutableArray<Diagnostic>.Empty,
                interfaces ?? Array.Empty<InterfaceImplementationResult>(),
                interfaceMembers ?? Array.Empty<InterfaceMemberImplementationResult>() );

        public static AdviceImplementationResult Success( AdviceOutcome outcome, IDeclaration newDeclaration )
            => new( outcome, newDeclaration.ToTypedRef(), ImmutableArray<Diagnostic>.Empty, Array.Empty<InterfaceImplementationResult>(), Array.Empty<InterfaceMemberImplementationResult>() );

        public static AdviceImplementationResult Ignored => new( AdviceOutcome.Ignored, default, ImmutableArray<Diagnostic>.Empty, Array.Empty<InterfaceImplementationResult>(), Array.Empty<InterfaceMemberImplementationResult>() );

        public static AdviceImplementationResult Failed( Diagnostic diagnostic ) => Failed( ImmutableArray.Create( diagnostic ) );

        public static AdviceImplementationResult Failed( ImmutableArray<Diagnostic> diagnostics ) => new( AdviceOutcome.Error, default, diagnostics, Array.Empty<InterfaceImplementationResult>(), Array.Empty<InterfaceMemberImplementationResult>() );
    }
}