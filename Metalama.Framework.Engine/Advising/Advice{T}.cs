// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal abstract class Advice<T> : Advice
    where T : AdviceResult, new()
{
    /// <summary>
    /// Applies the advice on the given compilation and returns the set of resulting transformations and diagnostics.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="compilation">Input compilation.</param>
    /// <param name="addTransformation"></param>
    /// <returns>Advice result containing transformations and diagnostics.</returns>
    protected abstract T Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation );

    /// <summary>
    /// Initializes the advice. Executed before any advices are executed.
    /// </summary>
    /// <remarks>
    /// The advice should only report diagnostics that do not take into account the target declaration(s).
    /// </remarks>
    protected virtual void Initialize( in ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder ) { }

    /// <summary>
    /// Validates the advice. Executed only if initialization passed, before implementing the advice.
    /// </summary>
    protected virtual void Validate( in ProjectServiceProvider serviceProvider, CompilationModel compilation, IDiagnosticAdder diagnosticAdder ) { }

    protected override AdviceResult ExecuteCore( IAdviceExecutionContext context ) => this.Execute( context );

    public new T Execute( IAdviceExecutionContext context )
    {
        List<ITransformation> transformations = new();

        // Initialize the advice. It should report errors for any situation that does not depend on the target declaration.
        // These errors are reported as exceptions.
        var initializationDiagnostics = new DiagnosticBag();
        this.Initialize( context.ServiceProvider, initializationDiagnostics );

        // Validate the advice against the current compilation.
        if ( !initializationDiagnostics.HasError )
        {
            this.Validate( context.ServiceProvider, context.CurrentCompilation, initializationDiagnostics );
        }

        if ( initializationDiagnostics.HasError() )
        {
            throw new DiagnosticException(
                "Errors have occured while creating advice.",
                initializationDiagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToImmutableArray() );
        }

        context.Diagnostics.Report( initializationDiagnostics );

        // Implement the advice. This should report errors for any situation that does depend on the target declaration.
        // These errors are reported as diagnostics.
        var adviceResult = this.Implement(
            context.ServiceProvider,
            context.CurrentCompilation,
            t =>
            {
                context.SetOrders( t );
                transformations.Add( t );
            } );

        // Set the compilation in whichi references must be resolved.
        adviceResult.Compilation = context.CurrentCompilation;

        // Report diagnostics.
        context.Diagnostics.Report( adviceResult.Diagnostics );

        context.IntrospectionListener?.AddAdviceResult( this.AspectInstance, this, adviceResult, context.CurrentCompilation );

        // Process outcome.
        switch ( adviceResult.Outcome )
        {
            case AdviceOutcome.Error:
                context.AspectInstance.Skip();

                break;

            case AdviceOutcome.Ignore:
                break;

            default:
                context.AddTransformations( transformations );

                if ( context.IntrospectionListener != null )
                {
                    adviceResult.Transformations = transformations.ToImmutableArray();
                }

                break;
        }

        return adviceResult;
    }

    protected Advice( AdviceConstructorParameters parameters ) : base( parameters ) { }

    protected T CreateFailedResult( Diagnostic diagnostic )
        => new() { Diagnostics = ImmutableArray.Create( diagnostic ), Outcome = AdviceOutcome.Error, AdviceKind = this.AdviceKind };

    protected T CreateFailedResult( ImmutableArray<Diagnostic> diagnostics )
        => new() { Diagnostics = diagnostics, Outcome = AdviceOutcome.Error, AdviceKind = this.AdviceKind };
}