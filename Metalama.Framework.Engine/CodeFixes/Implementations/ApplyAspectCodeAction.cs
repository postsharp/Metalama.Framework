// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes.Implementations;

internal class ApplyAspectCodeAction<TTarget> : ICodeAction
    where TTarget : class, IDeclaration
{
    public TTarget TargetDeclaration { get; }

    public IAspect<TTarget> Aspect { get; }

    public ApplyAspectCodeAction( TTarget targetDeclaration, IAspect<TTarget> aspect )
    {
        this.TargetDeclaration = targetDeclaration;
        this.Aspect = aspect;
    }

    public Task ExecuteAsync( CodeActionContext context )
    {
        var compilation = context.Compilation;

        var targetSymbol = this.TargetDeclaration.ToRef().GetSymbol( compilation.Compilation );

        if ( targetSymbol == null )
        {
            throw new ArgumentOutOfRangeException( nameof(this.TargetDeclaration), "The declaration is not declared in source." );
        }

        var aspectClass = (AspectClass) context.PipelineConfiguration.BoundAspectClasses.Single<IBoundAspectClass>( c => c.Type == this.Aspect.GetType() );

        if ( !LiveTemplateAspectPipeline.TryExecute(
                context.ServiceProvider,
                context.PipelineConfiguration.Domain,
                context.PipelineConfiguration,
                _ => aspectClass,
                PartialCompilation.CreatePartial( compilation.Compilation, targetSymbol.GetPrimaryDeclaration()!.SyntaxTree ),
                targetSymbol,
                NullDiagnosticAdder.Instance,
                CancellationToken.None,
                out var outputCompilation ) )
        {
            return Task.FromResult( false );
        }
        else
        {
            context.ApplyModifications( outputCompilation );

            return Task.FromResult( true );
        }
    }
}