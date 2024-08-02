// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.LiveTemplates;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes.Implementations;

internal sealed class ApplyAspectCodeAction<TTarget> : ICodeAction
    where TTarget : class, IDeclaration
{
    public TTarget TargetDeclaration { get; }

    private IAspect<TTarget> Aspect { get; }

    public ApplyAspectCodeAction( TTarget targetDeclaration, IAspect<TTarget> aspect )
    {
        this.TargetDeclaration = targetDeclaration;
        this.Aspect = aspect;
    }

    public async Task ExecuteAsync( CodeActionContext context )
    {
        var compilation = context.Compilation;

        var targetSymbol = this.TargetDeclaration.ToRef().GetSymbol( compilation.Compilation );

        if ( targetSymbol == null )
        {
            throw new ArgumentOutOfRangeException( nameof(this.TargetDeclaration), "The declaration is not declared in source." );
        }

        var aspectClass = (AspectClass) context.PipelineConfiguration.BoundAspectClasses.Single<IBoundAspectClass>( c => c.Type == this.Aspect.GetType() );

        var result = await LiveTemplateAspectPipeline.ExecuteAsync(
            context.ServiceProvider,
            context.PipelineConfiguration.Domain,
            context.PipelineConfiguration,
            _ => aspectClass,
            PartialCompilation.CreatePartial( compilation.Compilation, targetSymbol.GetPrimaryDeclaration()!.SyntaxTree ),
            targetSymbol,
            NullDiagnosticAdder.Instance,
            context.IsComputingPreview,
            context.CancellationToken.ToTestable() );

        if ( result.IsSuccessful )
        {
            context.ApplyModifications( result.Value );
        }
    }
}