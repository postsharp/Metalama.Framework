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
        public IReadOnlyCollection<ImplementedInterface> Interfaces { get; }

        public IReadOnlyCollection<ImplementedInterfaceMember> InterfaceMembers { get; }

        private AdviceImplementationResult( AdviceOutcome outcome, in Ref<IDeclaration> newDeclaration, ImmutableArray<Diagnostic> diagnostics, IReadOnlyCollection<ImplementedInterface> interfaces, IReadOnlyCollection<ImplementedInterfaceMember> interfaceMembers )
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
            IReadOnlyCollection<ImplementedInterface>? interfaces = null,
            IReadOnlyCollection<ImplementedInterfaceMember>? interfaceMembers = null )
            => new(
                outcome,
                newDeclaration,
                diagnostics ?? ImmutableArray<Diagnostic>.Empty,
                interfaces ?? Array.Empty<ImplementedInterface>(),
                interfaceMembers ?? Array.Empty<ImplementedInterfaceMember>() );

        public static AdviceImplementationResult Success( AdviceOutcome outcome, IDeclaration newDeclaration )
            => new( outcome, newDeclaration.ToTypedRef(), ImmutableArray<Diagnostic>.Empty, Array.Empty<ImplementedInterface>(), Array.Empty<ImplementedInterfaceMember>() );

        public static AdviceImplementationResult Ignored => new( AdviceOutcome.Ignored, default, ImmutableArray<Diagnostic>.Empty, Array.Empty<ImplementedInterface>(), Array.Empty<ImplementedInterfaceMember>() );

        public static AdviceImplementationResult Failed( Diagnostic diagnostic ) => Failed( ImmutableArray.Create( diagnostic ) );

        public static AdviceImplementationResult Failed( ImmutableArray<Diagnostic> diagnostics ) => new( AdviceOutcome.Error, default, diagnostics, Array.Empty<ImplementedInterface>(), Array.Empty<ImplementedInterfaceMember>() );
    }
}