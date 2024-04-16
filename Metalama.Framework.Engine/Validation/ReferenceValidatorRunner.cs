// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Validation;

public class ReferenceValidatorRunner
{
    private readonly IConcurrentTaskRunner _concurrentTaskRunner;
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly UserCodeInvoker _userCodeInvoker;

    public ReferenceValidatorRunner( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._userCodeInvoker = this._serviceProvider.GetRequiredService<UserCodeInvoker>();
        this._concurrentTaskRunner = this._serviceProvider.GetRequiredService<IConcurrentTaskRunner>();
    }

    public Task RunReferenceValidatorsAsync(
        CompilationModel initialCompilation,
        SemanticModel semanticModel,
        UserDiagnosticSink diagnosticAdder,
        IReferenceValidatorProvider referenceValidatorProvider,
        CancellationToken cancellationToken )
    {
        // Collect all references.
        var referenceIndexBuilder = new ReferenceIndexBuilder( this._serviceProvider, referenceValidatorProvider.Properties );
        referenceIndexBuilder.Index( semanticModel, cancellationToken );

        // Run the validator.
        return this.RunValidatorsCoreAsync(
            initialCompilation,
            diagnosticAdder,
            referenceValidatorProvider,
            cancellationToken,
            referenceIndexBuilder.ToReadOnly() );
    }

    public async Task RunReferenceValidatorsAsync(
        CompilationModel initialCompilation,
        UserDiagnosticSink diagnosticAdder,
        IReferenceValidatorProvider referenceValidatorProvider,
        CancellationToken cancellationToken )
    {
        // Collect all references.
        var semanticModelProvider = initialCompilation.CompilationContext.SemanticModelProvider;
        var referenceIndexBuilder = new ReferenceIndexBuilder( this._serviceProvider, referenceValidatorProvider.Properties );

        await this._concurrentTaskRunner.RunConcurrentlyAsync(
            initialCompilation.PartialCompilation.SyntaxTrees.Values,
            syntaxTree => referenceIndexBuilder.Index(
                semanticModelProvider.GetSemanticModel( syntaxTree, true ),
                cancellationToken ),
            cancellationToken );

        // Run the validator.
        await this.RunValidatorsCoreAsync(
            initialCompilation,
            diagnosticAdder,
            referenceValidatorProvider,
            cancellationToken,
            referenceIndexBuilder.ToReadOnly() );
    }

    private async Task RunValidatorsCoreAsync(
        CompilationModel initialCompilation,
        UserDiagnosticSink diagnosticAdder,
        IReferenceValidatorProvider referenceValidatorProvider,
        CancellationToken cancellationToken,
        ReferenceIndex referenceIndex )
    {
        var indexerOptions = referenceValidatorProvider.Properties;

        // Analyze the references.
        var userCodeExecutionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, default, compilationModel: initialCompilation );
        await this._concurrentTaskRunner.RunConcurrentlyAsync( referenceIndex.ReferencedSymbols, AnalyzeReferencedSymbolsAsync, cancellationToken );

        Task AnalyzeReferencedSymbolsAsync( ReferenceIndex.ReferencedSymbolInfo symbolInfo )
            => AnalyzeReferencedSymbolsImplAsync( symbolInfo, symbolInfo.ReferencedSymbol, false );

        async Task AnalyzeReferencedSymbolsImplAsync( ReferenceIndex.ReferencedSymbolInfo references, ISymbol referencedSymbol, bool isBaseType )
        {
            // Iterate all validators interested in the referenced symbol.
            foreach ( var validator in referenceValidatorProvider.GetValidators( referencedSymbol ) )
            {
                if ( isBaseType && !validator.IncludeDerivedTypes )
                {
                    continue;
                }

                userCodeExecutionContext.Description = validator.Driver.GetUserCodeMemberInfo( validator );

                // Validate all references.
                await this._concurrentTaskRunner.RunConcurrentlyAsync( references.References, AnalyzeReferencingSymbols, cancellationToken );

                void AnalyzeReferencingSymbols( ReferenceIndex.ReferencingSymbolInfo reference )
                {
                    if ( (validator.ReferenceKinds & references.AllReferenceKinds) != 0 )
                    {
                        foreach ( var node in reference.Nodes )
                        {
                            if ( (validator.ReferenceKinds & node.ReferenceKinds) != 0 )
                            {
                                var referencingDeclaration = initialCompilation.Factory.GetDeclaration( reference.ReferencingSymbol );

                                validator.Validate(
                                    referencingDeclaration,
                                    node.Syntax,
                                    node.ReferenceKinds,
                                    diagnosticAdder,
                                    this._userCodeInvoker,
                                    userCodeExecutionContext );
                            }
                        }
                    }
                }
            }

            // Recurse on the base type.

            if ( referencedSymbol is INamedTypeSymbol { BaseType: { } baseType }
                 && indexerOptions.MustDescendIntoReferencedBaseTypes( references.AllReferenceKinds ) )
            {
                await AnalyzeReferencedSymbolsImplAsync( references, baseType, true );
            }

            // Recurse on the containing type.
            if ( referencedSymbol.ContainingType != null )
            {
                if ( indexerOptions.MustDescendIntoReferencedDeclaringType( references.AllReferenceKinds ) )
                {
                    await AnalyzeReferencedSymbolsImplAsync( references, referencedSymbol.ContainingType, false );
                }
            }
            else if ( referencedSymbol.ContainingNamespace is { IsGlobalNamespace: false } )
            {
                if ( indexerOptions.MustDescendIntoReferencedNamespace( references.AllReferenceKinds ) )
                {
                    await AnalyzeReferencedSymbolsImplAsync( references, referencedSymbol.ContainingNamespace, false );
                }
            }
            else if ( referencedSymbol.ContainingAssembly != null )
            {
                if ( indexerOptions.MustDescendIntoReferencedAssembly( references.AllReferenceKinds ) )
                {
                    await AnalyzeReferencedSymbolsImplAsync( references, referencedSymbol.ContainingAssembly, false );
                }
            }
        }
    }
}