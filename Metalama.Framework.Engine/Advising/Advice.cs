// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;

namespace Metalama.Framework.Engine.Advising;

internal abstract class Advice : IAspectDeclarationOrigin, IDiagnosticSource
{
    public IAspectInstanceInternal AspectInstance { get; }

    protected TemplateClassInstance TemplateInstance { get; }

    public IDeclaration TargetDeclaration { get; }

    /// <summary>
    /// Gets the compilation from which the advice was instantiated.
    /// </summary>
    public CompilationModel SourceCompilation { get; }

    public AspectLayerId AspectLayerId { get; }

    public abstract AdviceKind AdviceKind { get; }

    protected Advice( AdviceConstructorParameters parameters )
    {
#if DEBUG
        if ( parameters.TargetDeclaration.DeclaringAssembly.IsExternal )
        {
            throw new AssertionFailedException( $"Cannot override '{parameters.TargetDeclaration}' because it is external." );
        }
#endif
        this.AspectInstance = parameters.AspectInstance;
        this.TemplateInstance = parameters.TemplateInstance;
        this.TargetDeclaration = parameters.TargetDeclaration;
        this.SourceCompilation = parameters.SourceCompilation;
        this.AspectLayerId = new AspectLayerId( this.AspectInstance.AspectClass, parameters.LayerName );
    }

    IAspectInstance IAspectDeclarationOrigin.AspectInstance => this.AspectInstance;

    DeclarationOriginKind IDeclarationOrigin.Kind => DeclarationOriginKind.Aspect;

    bool IDeclarationOrigin.IsCompilerGenerated => false;

    string IDiagnosticSource.DiagnosticSourceDescription => $"{this.GetType().Name} supplied by {this.AspectInstance.DiagnosticSourceDescription}'";

    /// <summary>
    /// Parameter object containing parameters shared by constructors of all advice types.
    /// </summary>
    public record struct AdviceConstructorParameters(
        IAspectInstanceInternal AspectInstance,
        TemplateClassInstance TemplateInstance,
        IDeclaration TargetDeclaration,
        CompilationModel SourceCompilation,
        string? LayerName );

    /// <summary>
    /// Generic version of parameter object containing parameters shared by constructors of all advice types.
    /// </summary>
    public record struct AdviceConstructorParameters<T>(
        IAspectInstanceInternal AspectInstance,
        TemplateClassInstance TemplateClassInstance,
        T TargetDeclaration,
        CompilationModel SourceCompilation,
        string? LayerName )
        where T : IDeclaration
    {
        public static implicit operator AdviceConstructorParameters( AdviceConstructorParameters<T> parameters )
            => new(
                parameters.AspectInstance,
                parameters.TemplateClassInstance,
                parameters.TargetDeclaration,
                parameters.SourceCompilation,
                parameters.LayerName );
    }
}