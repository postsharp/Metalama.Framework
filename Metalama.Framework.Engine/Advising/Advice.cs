// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Advising;

internal abstract class Advice : IAspectDeclarationOrigin, IDiagnosticSource
{
    public IAspectInstanceInternal AspectInstance { get; }

    public TemplateClassInstance TemplateInstance { get; }

    public Ref<IDeclaration> TargetDeclaration { get; }

    public AspectLayerId AspectLayerId { get; }

    /// <summary>
    /// Gets the compilation from which the advice was instantiated.
    /// </summary>
    public ICompilation SourceCompilation { get; }

    public abstract AdviceKind AdviceKind { get; }

    protected Advice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        string? layerName )
    {
#if DEBUG
        if ( targetDeclaration.DeclaringAssembly.IsExternal )
        {
            throw new AssertionFailedException( $"Cannot override '{targetDeclaration}' because it is external." );
        }
#endif
        this.AspectInstance = aspectInstance;
        this.TemplateInstance = template;
        this.TargetDeclaration = targetDeclaration.AssertNotNull().ToTypedRef();
        this.SourceCompilation = sourceCompilation;
        this.AspectLayerId = new AspectLayerId( this.AspectInstance.AspectClass, layerName );
    }

    IAspectInstance IAspectDeclarationOrigin.AspectInstance => this.AspectInstance;

    DeclarationOriginKind IDeclarationOrigin.Kind => DeclarationOriginKind.Aspect;

    bool IDeclarationOrigin.IsCompilerGenerated => false;

    string IDiagnosticSource.DiagnosticSourceDescription => $"{this.GetType().Name} supplied by {this.AspectInstance.DiagnosticSourceDescription}'";

    public AdviceResult Execute( IAdviceExecutionContext context ) => this.ExecuteCore( context );

    protected abstract AdviceResult ExecuteCore( IAdviceExecutionContext context );
}