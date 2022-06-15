// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Advising
{
    internal class AdviceImplementationResult
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        // This property is used only by the introspection API.
        public ImmutableArray<ITransformation> Transformations { get; internal set; } = ImmutableArray<ITransformation>.Empty;

        public AdviceOutcome Outcome { get; }

        public Ref<IDeclaration> NewDeclaration { get; }

        private AdviceImplementationResult( AdviceOutcome outcome, Ref<IDeclaration> newDeclaration, ImmutableArray<Diagnostic> diagnostics )
        {
            this.Diagnostics = diagnostics;
            this.Outcome = outcome;
            this.NewDeclaration = newDeclaration;
        }

        public static AdviceImplementationResult Success( IDeclaration newDeclaration ) => Success( AdviceOutcome.Default, newDeclaration.ToTypedRef() );

        public static AdviceImplementationResult Success( AdviceOutcome outcome = AdviceOutcome.Default, Ref<IDeclaration> newDeclaration = default )
            => new( outcome, newDeclaration, ImmutableArray<Diagnostic>.Empty );

        public static AdviceImplementationResult Success( AdviceOutcome outcome, IDeclaration newDeclaration )
            => new( outcome, newDeclaration.ToTypedRef(), ImmutableArray<Diagnostic>.Empty );

        public static AdviceImplementationResult Ignored => new( AdviceOutcome.Ignored, default, ImmutableArray<Diagnostic>.Empty );

        public static AdviceImplementationResult Failed( Diagnostic diagnostic ) => Failed( ImmutableArray.Create( diagnostic ) );

        public static AdviceImplementationResult Failed( UserDiagnosticSink diagnostics ) => Failed( diagnostics.ToImmutable().ReportedDiagnostics );

        public static AdviceImplementationResult Failed( ImmutableArray<Diagnostic> diagnostics ) => new( AdviceOutcome.Error, default, diagnostics );
    }
}