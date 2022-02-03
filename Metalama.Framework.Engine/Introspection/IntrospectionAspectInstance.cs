// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectInstance : IIntrospectionAspectInstance
{
    public AspectInstanceResult AspectInstanceResult { get; }

    public CompilationModel Compilation { get; }

    public IntrospectionAspectInstanceFactory Factory { get; }

    public IntrospectionAspectInstance( AspectInstanceResult result, CompilationModel compilation, IntrospectionAspectInstanceFactory factory )
    {
        this.AspectInstanceResult = result;
        this.Compilation = compilation;
        this.Factory = factory;
    }

    public IAspect Aspect => this.AspectInstanceResult.AspectInstance.Aspect;

    IDeclaration IIntrospectionAspectInstance.TargetDeclaration => this.AspectInstanceResult.AspectInstance.TargetDeclaration.GetTarget( this.Compilation );

    IRef<IDeclaration> IAspectInstance.TargetDeclaration => this.AspectInstanceResult.AspectInstance.TargetDeclaration;

    public IAspectClass AspectClass => this.AspectInstanceResult.AspectInstance.AspectClass;

    public bool IsSkipped => this.AspectInstanceResult.AspectInstance.IsSkipped;

    public ImmutableArray<IAspectInstance> SecondaryInstances => this.AspectInstanceResult.AspectInstance.SecondaryInstances;

    public ImmutableArray<AspectPredecessor> Predecessors => this.AspectInstanceResult.AspectInstance.Predecessors;

    public IAspectState? State => this.AspectInstanceResult.AspectInstance.State;

    [Memo]
    public ImmutableArray<IIntrospectionDiagnostic> Diagnostics
        => this.AspectInstanceResult.Diagnostics.ReportedDiagnostics.ToReportedDiagnostics( this.Compilation, DiagnosticSource.Metalama );

    [Memo]
    public ImmutableArray<IDeclaration> IntroducedMembers
        => this.AspectInstanceResult.ProgrammaticAdvices.OfType<IIntroductionAdvice>().Select( x => x.Builder ).ToImmutableArray<IDeclaration>();
}