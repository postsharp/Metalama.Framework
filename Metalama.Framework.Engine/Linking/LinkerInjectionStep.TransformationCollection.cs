// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    /// <summary>
    /// Mutable collection of data representing processed transformations. Used during rewriting and when creating injection registry.
    /// </summary>
    private sealed class TransformationCollection
    {
        private readonly TransformationLinkerOrderComparer _comparer;
        private readonly ConcurrentBag<InjectedMember> _injectedMembers;
        private readonly ConcurrentDictionary<InsertPosition, List<InjectedMember>> _injectedMembersByInsertPosition;
        private readonly ConcurrentDictionary<BaseTypeDeclarationSyntax, List<LinkerInjectedInterface>> _injectedInterfacesByTargetTypeDeclaration;
        private readonly HashSet<VariableDeclaratorSyntax> _removedVariableDeclaratorSyntax;
        private readonly HashSet<PropertyDeclarationSyntax> _autoPropertyWithSynthesizedSetterSyntax;
        private readonly ConcurrentDictionary<PropertyDeclarationSyntax, List<AspectLinkerDeclarationFlags>> _additionalDeclarationFlags;
        private readonly HashSet<SyntaxNode> _nodesWithModifiedAttributes;
        private readonly ConcurrentDictionary<SyntaxNode, MemberLevelTransformations> _symbolMemberLevelTransformations;
        private readonly ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations> _introductionMemberLevelTransformations;
        private readonly ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> _builderToTransformationMap;

        private readonly ConcurrentDictionary<IMethodBase, List<InsertedStatement>> _insertedStatementsByTargetMethodBase;

        private readonly ConcurrentDictionary<IDeclaration, List<InjectedMember>> _injectedMembersByTargetDeclaration;
        private readonly ConcurrentDictionary<IDeclaration, IReadOnlyList<IntroduceParameterTransformation>> _introducedParametersByTargetDeclaration;

        private readonly ConcurrentDictionary<INamedType, LateTypeLevelTransformations> _lateTypeLevelTransformations;

        private readonly HashSet<ITransformation> _transformationsCausingAuxiliaryOverrides;

        public IReadOnlyCollection<InjectedMember> InjectedMembers => this._injectedMembers;

        public IReadOnlyDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> BuilderToTransformationMap => this._builderToTransformationMap;

        public IReadOnlyDictionary<IDeclaration, IReadOnlyList<IntroduceParameterTransformation>> IntroducedParametersByTargetDeclaration
            => this._introducedParametersByTargetDeclaration;

        public IReadOnlyDictionary<INamedType, LateTypeLevelTransformations> LateTypeLevelTransformations => this._lateTypeLevelTransformations;

        // ReSharper disable once InconsistentlySynchronizedField
        public ISet<ITransformation> TransformationsCausingAuxiliaryOverrides => this._transformationsCausingAuxiliaryOverrides;

        public TransformationCollection( CompilationModel finalCompilationModel, TransformationLinkerOrderComparer comparer )
        {
            this._comparer = comparer;
            this._injectedMembers = new ConcurrentBag<InjectedMember>();
            this._injectedMembersByInsertPosition = new ConcurrentDictionary<InsertPosition, List<InjectedMember>>();
            this._injectedInterfacesByTargetTypeDeclaration = new ConcurrentDictionary<BaseTypeDeclarationSyntax, List<LinkerInjectedInterface>>();
            this._removedVariableDeclaratorSyntax = new HashSet<VariableDeclaratorSyntax>();
            this._autoPropertyWithSynthesizedSetterSyntax = new HashSet<PropertyDeclarationSyntax>();
            this._additionalDeclarationFlags = new ConcurrentDictionary<PropertyDeclarationSyntax, List<AspectLinkerDeclarationFlags>>();
            this._nodesWithModifiedAttributes = new HashSet<SyntaxNode>();
            this._symbolMemberLevelTransformations = new ConcurrentDictionary<SyntaxNode, MemberLevelTransformations>();
            this._introductionMemberLevelTransformations = new ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations>();
            this._builderToTransformationMap = new ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation>();

            this._insertedStatementsByTargetMethodBase =
                new ConcurrentDictionary<IMethodBase, List<InsertedStatement>>( finalCompilationModel.Comparers.Default );

            this._injectedMembersByTargetDeclaration = new ConcurrentDictionary<IDeclaration, List<InjectedMember>>( finalCompilationModel.Comparers.Default );

            this._introducedParametersByTargetDeclaration =
                new ConcurrentDictionary<IDeclaration, IReadOnlyList<IntroduceParameterTransformation>>( finalCompilationModel.Comparers.Default );

            this._lateTypeLevelTransformations = new ConcurrentDictionary<INamedType, LateTypeLevelTransformations>( finalCompilationModel.Comparers.Default );
            this._transformationsCausingAuxiliaryOverrides = new HashSet<ITransformation>();
        }

        public void AddInjectedMember( InjectedMember injectedMember )
            => this.AddInjectedMember( injectedMember.Declaration.ToInsertPosition(), injectedMember );

        public void AddInjectedMembers( IInjectMemberTransformation injectMemberTransformation, IEnumerable<InjectedMember> injectedMembers )
        {
            foreach ( var injectedMember in injectedMembers )
            {
                this.AddInjectedMember( injectMemberTransformation.InsertPosition, injectedMember );
            }
        }

        private void AddInjectedMember( InsertPosition insertPosition, InjectedMember injectedMember )
        {
            // Injected member should always be root type member (not an accessor).
            Invariant.Assert( injectedMember.Declaration is not { ContainingDeclaration: IMember } );

            this._injectedMembers.Add( injectedMember );

            var nodes = this._injectedMembersByInsertPosition.GetOrAdd( insertPosition, _ => new List<InjectedMember>() );

            lock ( nodes )
            {
                nodes.Add( injectedMember );
            }

            var declarationInjectedMembers =
                this._injectedMembersByTargetDeclaration.GetOrAdd( injectedMember.Declaration, _ => new List<InjectedMember>() );

            lock ( declarationInjectedMembers )
            {
                declarationInjectedMembers.Add( injectedMember );
            }
        }

        public void AddInjectedInterface( IInjectInterfaceTransformation injectInterfaceTransformation, BaseTypeSyntax injectedInterface )
        {
            var targetTypeSymbol = ((INamedType) injectInterfaceTransformation.TargetDeclaration).GetSymbol();

            // Heuristic: select the file with the shortest path.
            var targetTypeDecl = (BaseTypeDeclarationSyntax) targetTypeSymbol.GetPrimaryDeclaration().AssertNotNull();

            var interfaceList =
                this._injectedInterfacesByTargetTypeDeclaration.GetOrAdd(
                    targetTypeDecl,
                    _ => new List<LinkerInjectedInterface>() );

            lock ( interfaceList )
            {
                interfaceList.Add( new LinkerInjectedInterface( injectInterfaceTransformation, injectedInterface ) );
            }
        }

        public void AddAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax declaration )
        {
            Invariant.Assert( declaration.IsAutoPropertyDeclaration() && !declaration.HasSetterAccessorDeclaration() );

            lock ( this._autoPropertyWithSynthesizedSetterSyntax )
            {
                this._autoPropertyWithSynthesizedSetterSyntax.Add( declaration );
            }
        }

        // ReSharper disable once UnusedMember.Local
        public void AddDeclarationWithAdditionalFlags( PropertyDeclarationSyntax declaration, AspectLinkerDeclarationFlags flags )
        {
            var list = this._additionalDeclarationFlags.GetOrAdd( declaration, _ => new List<AspectLinkerDeclarationFlags>() );

            lock ( list )
            {
                list.Add( flags );
            }
        }

        public void AddInsertedStatements( IMethodBase targetMethod, IReadOnlyList<InsertedStatement> statements )
        {
            // PERF: Synchronization should not be needed because we are in the same syntax tree (if not, this would be non-deterministic and thus wrong).
            //       Assertions should be added first.
            var statementList = this._insertedStatementsByTargetMethodBase.GetOrAdd( targetMethod, _ => new List<InsertedStatement>() );

            lock ( statementList )
            {
                statementList.AddRange( statements );
            }
        }

        public void AddRemovedSyntax( SyntaxNode removedSyntax )
        {
            switch ( removedSyntax )
            {
                case VariableDeclaratorSyntax variableDeclarator:
                    lock ( this._removedVariableDeclaratorSyntax )
                    {
                        this._removedVariableDeclaratorSyntax.Add( variableDeclarator );
                    }

                    break;

                default:
                    throw new AssertionFailedException( $"{removedSyntax.Kind()} is not supported removed syntax." );
            }
        }

        public void AddNodeWithModifiedAttributes( SyntaxNode node )
        {
            lock ( this._nodesWithModifiedAttributes )
            {
                this._nodesWithModifiedAttributes.Add( node );
            }
        }

        internal void AddIntroducedParameter( IntroduceParameterTransformation introduceParameterTransformation )
        {
            var parameterList = this._introducedParametersByTargetDeclaration.GetOrAdd(
                introduceParameterTransformation.TargetDeclaration,
                _ => new List<IntroduceParameterTransformation>() );

            lock ( parameterList )
            {
                ((List<IntroduceParameterTransformation>) parameterList).Add( introduceParameterTransformation );
            }
        }

        // ReSharper disable once InconsistentlySynchronizedField
        public bool IsRemovedSyntax( VariableDeclaratorSyntax variableDeclarator ) => this._removedVariableDeclaratorSyntax.Contains( variableDeclarator );

        // ReSharper disable once InconsistentlySynchronizedField
        public bool IsAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax propertyDeclaration )
            => this._autoPropertyWithSynthesizedSetterSyntax.Contains( propertyDeclaration );

        // ReSharper disable once InconsistentlySynchronizedField
        public bool IsNodeWithModifiedAttributes( SyntaxNode node ) => this._nodesWithModifiedAttributes.Contains( node );

        public IReadOnlyList<InjectedMember> GetInjectedMembersOnPosition( InsertPosition position )
        {
            if ( this._injectedMembersByInsertPosition.TryGetValue( position, out var injectedMembers ) )
            {
                // IMPORTANT - do not change the introduced node here.
                injectedMembers.Sort( InjectedMemberComparer.Instance );

                return injectedMembers;
            }

            return Array.Empty<InjectedMember>();
        }

        public IReadOnlyList<LinkerInjectedInterface> GetIntroducedInterfacesForTypeDeclaration( BaseTypeDeclarationSyntax typeDeclaration )
        {
            if ( this._injectedInterfacesByTargetTypeDeclaration.TryGetValue( typeDeclaration, out var interfaceList ) )
            {
                interfaceList.Sort( ( x, y ) => this._comparer.Compare( x.Transformation, y.Transformation ) );

                return interfaceList;
            }

            return Array.Empty<LinkerInjectedInterface>();
        }

        public AspectLinkerDeclarationFlags GetAdditionalDeclarationFlags( PropertyDeclarationSyntax declaration )
        {
            if ( this._additionalDeclarationFlags.TryGetValue( declaration, out var list ) )
            {
                var finalFlags = AspectLinkerDeclarationFlags.None;

                foreach ( var flags in list )
                {
                    finalFlags |= flags;
                }

                return finalFlags;
            }

            return AspectLinkerDeclarationFlags.None;
        }

        public bool TryGetMemberLevelTransformations( SyntaxNode node, [NotNullWhen( true )] out MemberLevelTransformations? memberLevelTransformations )
            => this._symbolMemberLevelTransformations.TryGetValue( node, out memberLevelTransformations );

        public bool TryGetMemberLevelTransformations(
            IDeclarationBuilder builder,
            [NotNullWhen( true )] out MemberLevelTransformations? memberLevelTransformations )
            => this._introductionMemberLevelTransformations.TryGetValue( builder, out memberLevelTransformations );

        public async Task FinalizeAsync(
            IConcurrentTaskRunner concurrentTaskRunner,
            CancellationToken cancellationToken )
        {
            await concurrentTaskRunner.RunConcurrentlyAsync(
                this._introductionMemberLevelTransformations.Values,
                t => t.Sort(),
                cancellationToken );

            await concurrentTaskRunner.RunConcurrentlyAsync(
                this._symbolMemberLevelTransformations.Values,
                t => t.Sort(),
                cancellationToken );
        }

        public void AddIntroduceTransformation( IDeclarationBuilder declarationBuilder, IIntroduceDeclarationTransformation introduceDeclarationTransformation )
        {
            var wasAdded = this._builderToTransformationMap.TryAdd( declarationBuilder, introduceDeclarationTransformation );

            Invariant.Assert( wasAdded );
        }

        public void AddTransformationCausingAuxiliaryOverride( ITransformation causalTransformation )
        {
            lock ( this._transformationsCausingAuxiliaryOverrides )
            {
                this._transformationsCausingAuxiliaryOverrides.Add( causalTransformation );
            }
        }

        public bool TryGetIntroduceDeclarationTransformation(
            IDeclarationBuilder replacedBuilder,
            [NotNullWhen( true )] out IIntroduceDeclarationTransformation? introduceDeclarationTransformation )
            => this._builderToTransformationMap.TryGetValue( replacedBuilder, out introduceDeclarationTransformation );

        public MemberLevelTransformations GetOrAddMemberLevelTransformations( SyntaxNode declarationSyntax )
            => this._symbolMemberLevelTransformations.GetOrAdd( declarationSyntax, static _ => new MemberLevelTransformations() );

        public MemberLevelTransformations GetOrAddMemberLevelTransformations( IDeclarationBuilder declarationBuilder )
            => this._introductionMemberLevelTransformations.GetOrAdd( declarationBuilder, static _ => new MemberLevelTransformations() );

        public LateTypeLevelTransformations GetOrAddLateTypeLevelTransformations( INamedType type )
            => this._lateTypeLevelTransformations.GetOrAdd( type, static _ => new LateTypeLevelTransformations() );

        internal IReadOnlyList<StatementSyntax> GetInjectedEntryStatements( InjectedMember injectedMember )
            => this.GetInjectedEntryStatements( (IMethodBase) injectedMember.Declaration, injectedMember );

        internal IReadOnlyList<StatementSyntax> GetInjectedEntryStatements( IMethodBase sourceMethodBase )
            => this.GetInjectedEntryStatements( sourceMethodBase, null );

        internal IReadOnlyList<StatementSyntax> GetInjectedEntryStatements( IMethodBase targetMethodBase, InjectedMember? targetInjectedMember )
        {
            // PERF: Iterating and reversing should be avoided.
            if ( !this._insertedStatementsByTargetMethodBase.TryGetValue( targetMethodBase, out var insertedStatements ) )
            {
                return ImmutableArray<StatementSyntax>.Empty;
            }

            bool hasInjectedMembers;
            MemberLayerIndex? bottomBound;
            MemberLayerIndex? topBound;

            var rootMember =
                targetMethodBase switch
                {
                    IMethod { ContainingDeclaration: IProperty property } => property,
                    IMethod { ContainingDeclaration: IIndexer indexer } => indexer,
                    _ => (IMember) targetMethodBase
                };

            // If trying to get inserted statements for a source declaration, we need to first find the first injected member.
            if ( !this._injectedMembersByTargetDeclaration.TryGetValue( rootMember, out var injectedMembers ) )
            {
                hasInjectedMembers = false;

                if ( targetInjectedMember == null )
                {
                    bottomBound = null;
                    topBound = null;
                }
                else
                {
                    throw new AssertionFailedException( $"Missing injected member for {rootMember}" );
                }
            }
            else
            {
                injectedMembers = injectedMembers.ToOrderedList( x => GetTransformationMemberLayerIndex( x.Transformation ) );

                hasInjectedMembers = true;

                if ( targetInjectedMember == null )
                {
                    bottomBound = null;
                    topBound = GetTransformationMemberLayerIndex( injectedMembers.First().Transformation );
                }
                else
                {
                    var targetInjectedMemberIndex = injectedMembers.IndexOf( targetInjectedMember );

                    if ( targetInjectedMemberIndex < 0 )
                    {
                        throw new AssertionFailedException( $"Missing injected members for {targetMethodBase}" );
                    }

                    bottomBound = GetTransformationMemberLayerIndex( targetInjectedMember.Transformation );

                    topBound =
                        targetInjectedMemberIndex >= injectedMembers.Count - 1
                            ? null
                            : GetTransformationMemberLayerIndex( injectedMembers[targetInjectedMemberIndex + 1].Transformation );
                }
            }

            var statements = new List<StatementSyntax>();

            if ( targetMethodBase is IConstructor )
            {
                if ( (!hasInjectedMembers && targetInjectedMember == null) || (hasInjectedMembers && targetInjectedMember == injectedMembers![^1]) )
                {
                    // Return initializer statements source members with no overrides or to the last override.
                    var initializerStatements =
                        insertedStatements
                            .Where( s => s.Kind == InsertedStatementKind.Initializer )
                            .Select( s => s );

                    var orderedInitializerStatements = OrderInitializerStatements( initializerStatements );

                    statements.AddRange( orderedInitializerStatements.Select( s => s.Statement ) );
                }
            }

            // For non-initializer statements we have to select a range of statements that fits this injected member.
            var inputContractStatements =
                insertedStatements
                    .Where(
                        s =>
                            s.Kind == InsertedStatementKind.InputContract
                            && (bottomBound == null || GetTransformationMemberLayerIndex( s.Transformation ) >= bottomBound)
                            && (topBound == null || GetTransformationMemberLayerIndex( s.Transformation ) < topBound) );

            var orderedInputContractStatements = OrderInputContractStatements( inputContractStatements );

            statements.AddRange(
                orderedInputContractStatements.Select(
                    s =>
                        s.Statement switch
                        {
                            BlockSyntax block => block.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                            _ => s.Statement
                        } ) );

            return statements;
        }

        internal IReadOnlyList<StatementSyntax> GetInjectedExitStatements( InjectedMember injectedMember )
            => this.GetInjectedExitStatements( (IMethodBase) injectedMember.Declaration, injectedMember );

        internal IReadOnlyList<StatementSyntax> GetInjectedExitStatements( IMethodBase targetMethodBase, InjectedMember targetInjectedMember )
        {
            // PERF: Iterating and reversing should be avoided.
            if ( !this._insertedStatementsByTargetMethodBase.TryGetValue( targetMethodBase, out var insertedStatements ) )
            {
                return ImmutableArray<StatementSyntax>.Empty;
            }

            MemberLayerIndex bottomBound;
            MemberLayerIndex? topBound;

            var rootMember =
                targetMethodBase switch
                {
                    IMethod { ContainingDeclaration: IProperty property } => property,
                    IMethod { ContainingDeclaration: IIndexer indexer } => indexer,
                    _ => (IMember) targetMethodBase
                };

            // If trying to get inserted statements for a source declaration, we need to first find the first injected member.
            if ( !this._injectedMembersByTargetDeclaration.TryGetValue( rootMember, out var injectedMembers ) )
            {
                throw new AssertionFailedException( $"Missing injected member for {targetMethodBase} (exit statements are not supported on source members)." );
            }
            else
            {
                injectedMembers = injectedMembers.ToOrderedList( x => GetTransformationMemberLayerIndex( x.Transformation ) );

                var targetInjectedMemberIndex = injectedMembers.IndexOf( targetInjectedMember );

                if ( targetInjectedMemberIndex < 0 )
                {
                    throw new AssertionFailedException( $"Missing injected members for {targetMethodBase}" );
                }

                bottomBound = GetTransformationMemberLayerIndex( targetInjectedMember.Transformation );

                topBound =
                    targetInjectedMemberIndex >= injectedMembers.Count - 1
                        ? null
                        : GetTransformationMemberLayerIndex( injectedMembers[targetInjectedMemberIndex + 1].Transformation );
            }

            var statements = new List<StatementSyntax>();

            // For non-initializer statements we have to select a range of statements that fits this injected member.
            var outputContractStatements =
                insertedStatements
                    .Where(
                        s =>
                            s.Kind == InsertedStatementKind.OutputContract
                            && GetTransformationMemberLayerIndex( s.Transformation ) >= bottomBound
                            && (topBound == null || GetTransformationMemberLayerIndex( s.Transformation ) < topBound) );

            var orderedOutputContractStatements = OrderOutputContractStatements( outputContractStatements );

            statements.AddRange(
                orderedOutputContractStatements.Select(
                    s =>
                        s.Statement switch
                        {
                            BlockSyntax block => block.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                            _ => s.Statement
                        } ) );

            return statements;
        }

        private static IEnumerable<InsertedStatement> OrderInitializerStatements( IEnumerable<InsertedStatement> statements )

            // Initializers of separate declarations should precede initializers of the type.
            => statements
                .OrderBy(
                    s => s.ContextDeclaration switch
                    {
                        IMember => 0,
                        INamedType => 1,
                        _ => throw new AssertionFailedException( $"Unexpected declaration: '{s.ContextDeclaration}'." )
                    } )
                .ThenBy( s => (s.ContextDeclaration as IMember)?.ToDisplayString() );

        private static IEnumerable<InsertedStatement> OrderInputContractStatements( IEnumerable<InsertedStatement> statements )
            =>

                // Makes sure that the order is not changed when override is added in the middle of aspects that insert statements.
                statements
                    .OrderBy(
                        s => s.ContextDeclaration switch
                        {
                            IParameter { IsReturnParameter: false } parameter => parameter.Index, // Parameters are checked in order they appear in code.
                            _ => throw new AssertionFailedException( $"Unexpected declaration: '{s.ContextDeclaration}'." )
                        } )
                    .ThenByDescending( s => s.Transformation.OrderWithinPipeline )
                    .ThenByDescending( s => s.Transformation.OrderWithinPipelineStepAndType )
                    .ThenBy( s => s.Transformation.OrderWithinPipelineStepAndTypeAndAspectInstance );

        private static IEnumerable<InsertedStatement> OrderOutputContractStatements( IEnumerable<InsertedStatement> statements )
            =>

                // Makes sure that the order is not changed when override is added in the middle of aspects that insert statements.
                statements
                    .OrderBy(
                        s => s.ContextDeclaration switch
                        {
                            IParameter { IsReturnParameter: false } parameter => parameter.Index, // Parameters are checked in order they appear in code.
                            IParameter { IsReturnParameter: true, ContainingDeclaration: IMethod method } =>
                                method.Parameters.Count, // Method return value contracts are ordered after other parameters
                            _ => throw new AssertionFailedException( $"Unexpected declaration: '{s.ContextDeclaration}'." )
                        } )
                    .ThenByDescending( s => s.Transformation.OrderWithinPipeline )
                    .ThenByDescending( s => s.Transformation.OrderWithinPipelineStepAndType )
                    .ThenBy( s => s.Transformation.OrderWithinPipelineStepAndTypeAndAspectInstance );

        private static MemberLayerIndex GetTransformationMemberLayerIndex( ITransformation? transformation )
            => transformation != null
                ? new MemberLayerIndex(
                    transformation.OrderWithinPipeline,
                    transformation.OrderWithinPipelineStepAndType,
                    transformation.OrderWithinPipelineStepAndTypeAndAspectInstance )
                : new MemberLayerIndex( 0, 0, 0 );
    }
}