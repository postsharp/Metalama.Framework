// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectInstance : IIntrospectionAspectInstance
{
    public IAspectInstance AspectInstance { get; }

    public AspectInstanceResult? AspectInstanceResult { get; internal set; }

    public CompilationModel Compilation { get; }

    public IntrospectionAspectInstanceFactory Factory { get; }

    public IntrospectionAspectInstance( IAspectInstance aspectInstance, CompilationModel compilation, IntrospectionAspectInstanceFactory factory )
    {
        this.AspectInstance = aspectInstance;
        this.Compilation = compilation;
        this.Factory = factory;
    }

    public IAspect Aspect => this.AspectInstance.Aspect;

    public List<IntrospectionAdvice> Advice { get; } = new();

    IReadOnlyList<IIntrospectionAdvice> IIntrospectionAspectInstance.Advice => this.Advice;

    IDeclaration IIntrospectionAspectInstance.TargetDeclaration => this.AspectInstance.TargetDeclaration.GetTarget( this.Compilation );

    IRef<IDeclaration> IAspectPredecessor.TargetDeclaration => this.AspectInstance.TargetDeclaration;

    public IAspectClass AspectClass => this.AspectInstance.AspectClass;

    public bool IsSkipped => this.AspectInstance.IsSkipped;

    public ImmutableArray<IAspectInstance> SecondaryInstances => this.AspectInstance.SecondaryInstances;

    public ImmutableArray<AspectPredecessor> Predecessors => this.AspectInstance.Predecessors;

    public IAspectState? AspectState => this.AspectInstance.AspectState;

    [Memo]
    public ImmutableArray<IIntrospectionDiagnostic> Diagnostics
        => (this.AspectInstanceResult ?? throw new InvalidOperationException()).Diagnostics.ReportedDiagnostics.ToReportedDiagnostics(
            this.Compilation,
            DiagnosticSource.Metalama );

    public int PredecessorDegree => this.AspectInstance.PredecessorDegree;
}