// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceValidatorRunner
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
        var referenceIndexBuilder = new ReferenceIndexBuilder( this._serviceProvider, referenceValidatorProvider.Options );
        referenceIndexBuilder.IndexSemanticModel( semanticModel, cancellationToken );

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
        var referenceIndexBuilder = new ReferenceIndexBuilder( this._serviceProvider, referenceValidatorProvider.Options );

        await this._concurrentTaskRunner.RunConcurrentlyAsync(
            initialCompilation.PartialCompilation.SyntaxTrees.Values,
            syntaxTree => referenceIndexBuilder.IndexSyntaxTree(
                syntaxTree,
                semanticModelProvider,
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
        // Analyze the references.
        var userCodeExecutionContext = new UserCodeExecutionContext( this._serviceProvider, diagnosticAdder, default, compilationModel: initialCompilation );
        await this._concurrentTaskRunner.RunConcurrentlyAsync( referenceIndex.ReferencedSymbols, AnalyzeReferencedSymbolsAsync, cancellationToken );

        Task AnalyzeReferencedSymbolsAsync( ReferencedSymbolInfo symbolInfo )
            => AnalyzeReferencedSymbolsImplAsync( symbolInfo, symbolInfo.ReferencedSymbol, false );

        async Task AnalyzeReferencedSymbolsImplAsync( ReferencedSymbolInfo references, ISymbol referencedSymbol, bool isBaseType )
        {
            var allReferences = references.GetAllReferences( ChildKinds.All ).ToReadOnlyList();

            var validatorsByGranularity = referenceValidatorProvider
                .GetValidators( referencedSymbol )
                .Where( v => !isBaseType || v.Properties.IncludeDerivedTypes )
                .GroupBy( v => v.Granularity );

            foreach ( var validatorGroup in validatorsByGranularity )
            {
                // Static local functions to get the grouping factor.
                static ISymbol? GetNamespace( ReferencingSymbolInfo symbol ) => symbol.ReferencingSymbol.ContainingNamespace;

                static ISymbol? GetAssembly( ReferencingSymbolInfo symbol ) => symbol.ReferencingSymbol.ContainingAssembly;

                static ISymbol? GetParentType( ReferencingSymbolInfo symbol ) => symbol.ReferencingSymbol.GetClosestContainingType();

                static ISymbol? GetTopLevelType( ReferencingSymbolInfo symbol )
                    => symbol.ReferencingSymbol.GetClosestContainingType()?.GetTopmostContainingType();

                static ISymbol? GetMember( ReferencingSymbolInfo symbol ) => symbol.ReferencingSymbol.GetClosestContainingMember();

                static ISymbol GetDeclaration( ReferencingSymbolInfo symbol ) => symbol.ReferencingSymbol;

                // Choose the grouping factor according to the desired granularity.
                var getGroupingKeyFunc = (Func<ReferencingSymbolInfo, ISymbol?>) (validatorGroup.Key switch
                {
                    ReferenceGranularity.Compilation => GetAssembly,
                    ReferenceGranularity.Namespace => GetNamespace,
                    ReferenceGranularity.Type => GetParentType,
                    ReferenceGranularity.TopLevelType => GetTopLevelType,
                    ReferenceGranularity.Member => GetMember,
                    _ => GetDeclaration
                });

                // Group the references.
                var groupedReferences = allReferences.GroupBy( getGroupingKeyFunc ).Cache();

                // Iterate all validators.
                foreach ( var validator in validatorGroup )
                {
                    userCodeExecutionContext.Description = validator.Driver.GetUserCodeMemberInfo( validator );

                    // Select groups that have at least one relevant node for the validator. 
                    var groupedReferencesForValidator =
                        groupedReferences.Where( g => g.Any( r => (r.Nodes.ReferenceKinds & validator.Properties.ReferenceKinds) != 0 ) );

#pragma warning disable CS0612 // Type or member is obsolete
                    if ( validatorGroup.Key == ReferenceGranularity.SyntaxNode )
#pragma warning restore CS0612 // Type or member is obsolete
                    {
                        // For backward compatibility, we have to flatten the group and do one call per syntax node.
                        var flattenedReferences = groupedReferences
                            .Where( g => g.Key != null )
                            .SelectMany<IGrouping<ISymbol?, ReferencingSymbolInfo>, (ISymbol?, ReferencingSymbolInfo)>(
                                g => g.SelectMany<ReferencingSymbolInfo, (ISymbol?, ReferencingSymbolInfo)>(
                                    x => x.Nodes.SelectAsReadOnlyCollection<ReferencingNode, (ISymbol?, ReferencingSymbolInfo)>(
                                        n => (g.Key, new ReferencingSymbolInfo( g.Key!, [n] )) ) ) );

                        // Run the validation concurrently.
                        await this._concurrentTaskRunner.RunConcurrentlyAsync(
                            flattenedReferences,
                            AnalyzeReferencingSymbols,
                            cancellationToken );

                        void AnalyzeReferencingSymbols( (ISymbol? Symbol, ReferencingSymbolInfo Reference) reference )
                        {
                            if ( reference.Symbol == null )
                            {
                                return;
                            }

                            var referencingDeclaration = initialCompilation.Factory.GetDeclaration( reference.Symbol );

                            validator.Validate(
                                referencingDeclaration,
                                diagnosticAdder,
                                this._userCodeInvoker,
                                userCodeExecutionContext,
                                new[] { reference.Reference } );
                        }
                    }
                    else
                    {
                        // Run the validation concurrently.
                        await this._concurrentTaskRunner.RunConcurrentlyAsync(
                            groupedReferencesForValidator,
                            AnalyzeReferencingSymbols,
                            cancellationToken );

                        void AnalyzeReferencingSymbols( IGrouping<ISymbol?, ReferencingSymbolInfo> referenceGroup )
                        {
                            if ( referenceGroup.Key == null )
                            {
                                return;
                            }

                            var referencingDeclaration = initialCompilation.Factory.GetDeclaration( referenceGroup.Key );

                            validator.Validate(
                                referencingDeclaration,
                                diagnosticAdder,
                                this._userCodeInvoker,
                                userCodeExecutionContext,
                                referenceGroup );
                        }
                    }
                }
            }
        }
    }
}