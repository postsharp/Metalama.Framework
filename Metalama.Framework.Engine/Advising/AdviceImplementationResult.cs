// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
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

        public IRef<IDeclaration>? NewDeclaration { get; }

        // These properties are used only for introduce interface advice.
        public IReadOnlyCollection<IInterfaceImplementationResult> Interfaces { get; }

        public IReadOnlyCollection<IInterfaceMemberImplementationResult> InterfaceMembers { get; }

        private AdviceImplementationResult( AdviceOutcome outcome, IRef<IDeclaration>? newDeclaration, ImmutableArray<Diagnostic> diagnostics, IReadOnlyCollection<IInterfaceImplementationResult> interfaces, IReadOnlyCollection<IInterfaceMemberImplementationResult> interfaceMembers )
        {
            this.Diagnostics = diagnostics;
            this.Outcome = outcome;
            this.NewDeclaration = newDeclaration;
            this.Interfaces = interfaces;
            this.InterfaceMembers = interfaceMembers;
        }

        public static AdviceImplementationResult Success( IDeclaration newDeclaration ) => Success( AdviceOutcome.Default, newDeclaration.ToRef() );

        public static AdviceImplementationResult Success(
            AdviceOutcome outcome = AdviceOutcome.Default,
            IRef<IDeclaration>? newDeclaration = null,
            ImmutableArray<Diagnostic>? diagnostics = null,
            IReadOnlyCollection<IInterfaceImplementationResult>? interfaces = null,
            IReadOnlyCollection<IInterfaceMemberImplementationResult>? interfaceMembers = null )
            => new(
                outcome,
                newDeclaration,
                diagnostics ?? ImmutableArray<Diagnostic>.Empty,
                interfaces ?? Array.Empty<IInterfaceImplementationResult>(),
                interfaceMembers ?? Array.Empty<IInterfaceMemberImplementationResult>() );

        public static AdviceImplementationResult Success( AdviceOutcome outcome, IDeclaration newDeclaration )
            => new( outcome, newDeclaration.ToRef(), ImmutableArray<Diagnostic>.Empty, Array.Empty<IInterfaceImplementationResult>(), Array.Empty<IInterfaceMemberImplementationResult>() );

        public static AdviceImplementationResult Ignored( IDeclaration existingDeclaration )
            => Ignored( existingDeclaration.ToRef() );

        public static AdviceImplementationResult Ignored( IRef<IDeclaration> existingDeclaration )
            => new( AdviceOutcome.Ignore, existingDeclaration, ImmutableArray<Diagnostic>.Empty, Array.Empty<IInterfaceImplementationResult>(), Array.Empty<IInterfaceMemberImplementationResult>() );

        public static AdviceImplementationResult Failed( Diagnostic diagnostic ) => Failed( ImmutableArray.Create( diagnostic ) );

        public static AdviceImplementationResult Failed( ImmutableArray<Diagnostic> diagnostics ) => new( AdviceOutcome.Error, null, diagnostics, Array.Empty<IInterfaceImplementationResult>(), Array.Empty<IInterfaceMemberImplementationResult>() );
    }
}