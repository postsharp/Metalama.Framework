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

        private readonly ConcurrentDictionary<IDeclaration, List<InsertedStatement>> _insertedStatementsByTargetDeclaration;

        private readonly ConcurrentDictionary<IDeclaration, List<InjectedMember>> _injectedMembersByTargetDeclaration;
        private readonly ConcurrentDictionary<IDeclaration, IReadOnlyList<IntroduceParameterTransformation>> _introducedParametersByTargetDeclaration;

        private readonly ConcurrentDictionary<INamedType, LateTypeLevelTransformations> _lateTypeLevelTransformations;

        public IReadOnlyCollection<InjectedMember> InjectedMembers => this._injectedMembers;

        public IReadOnlyDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> BuilderToTransformationMap => this._builderToTransformationMap;

        public IReadOnlyDictionary<IDeclaration, IReadOnlyList<IntroduceParameterTransformation>> IntroducedParametersByTargetDeclaration
            => this._introducedParametersByTargetDeclaration;

        public IReadOnlyDictionary<INamedType, LateTypeLevelTransformations> LateTypeLevelTransformations => this._lateTypeLevelTransformations;

        public TransformationCollection( CompilationModel finalCompilationModel, TransformationLinkerOrderComparer comparer )
        {
            this._comparer = comparer;
            this._injectedMembers = new();
            this._injectedMembersByInsertPosition = new();
            this._injectedInterfacesByTargetTypeDeclaration = new();
            this._removedVariableDeclaratorSyntax = new();
            this._autoPropertyWithSynthesizedSetterSyntax = new();
            this._additionalDeclarationFlags = new();
            this._nodesWithModifiedAttributes = new();
            this._symbolMemberLevelTransformations = new();
            this._introductionMemberLevelTransformations = new();
            this._builderToTransformationMap = new();
            this._insertedStatementsByTargetDeclaration = new( finalCompilationModel.Comparers.Default );
            this._injectedMembersByTargetDeclaration = new( finalCompilationModel.Comparers.Default);
            this._introducedParametersByTargetDeclaration = new( finalCompilationModel.Comparers.Default );
            this._lateTypeLevelTransformations = new( finalCompilationModel.Comparers.Default );
        }

        public void AddInjectedMember( InjectedMember injectedMember )
        {
            this._injectedMembers.Add( injectedMember );

            var nodes = this._injectedMembersByInsertPosition.GetOrAdd( injectedMember.Declaration.ToInsertPosition(), _ => new List<InjectedMember>() );

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

        public void AddInjectedMembers( IInjectMemberTransformation injectMemberTransformation, IEnumerable<InjectedMember> injectedMembers )
        {
            foreach ( var injectedMember in injectedMembers )
            {
                this._injectedMembers.Add( injectedMember );

                var nodes = this._injectedMembersByInsertPosition.GetOrAdd( injectMemberTransformation.InsertPosition, _ => new List<InjectedMember>() );

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

        public void AddInsertedStatements( IMember targetMember, IReadOnlyList<InsertedStatement> statements )
        {
            // PERF: Synchronization should not be needed because we are in the same syntax tree (if not, this would be non-deterministic and thus wrong).
            //       Assertions should be added first.
            var statementList = this._insertedStatementsByTargetDeclaration.GetOrAdd( targetMember, _ => new() );

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

        public bool IsRemovedSyntax( VariableDeclaratorSyntax variableDeclarator ) => this._removedVariableDeclaratorSyntax.Contains( variableDeclarator );

        public bool IsAutoPropertyWithSynthesizedSetter( PropertyDeclarationSyntax propertyDeclaration )
            => this._autoPropertyWithSynthesizedSetterSyntax.Contains( propertyDeclaration );

        public bool IsNodeWithModifiedAttributes( SyntaxNode node )
        {
            return this._nodesWithModifiedAttributes.Contains( node );
        }

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
        {
            return this._symbolMemberLevelTransformations.TryGetValue( node, out memberLevelTransformations );
        }

        public bool TryGetMemberLevelTransformations(
            IDeclarationBuilder builder,
            [NotNullWhen( true )] out MemberLevelTransformations? memberLevelTransformations )
        {
            return this._introductionMemberLevelTransformations.TryGetValue( builder, out memberLevelTransformations );
        }

        public bool HasMemberLevelTransformations( SyntaxNode syntax )
        {
            return this._symbolMemberLevelTransformations.ContainsKey( syntax );
        }

        public async Task FinalizeAsync(
            IConcurrentTaskRunner concurrentTaskRunner,
            CancellationToken cancellationToken )
        {
            await concurrentTaskRunner.RunInParallelAsync(
                this._introductionMemberLevelTransformations.Values,
                t => t.Sort(),
                cancellationToken );

            await concurrentTaskRunner.RunInParallelAsync(
                this._symbolMemberLevelTransformations.Values,
                t => t.Sort(),
                cancellationToken );
        }

        public void AddIntroduceTransformation( IDeclarationBuilder declarationBuilder, IIntroduceDeclarationTransformation introduceDeclarationTransformation )
        {
            var wasAdded = this._builderToTransformationMap.TryAdd( declarationBuilder, introduceDeclarationTransformation );

            Invariant.Assert( wasAdded );
        }

        public bool TryGetIntroduceDeclarationTransformation(
            IDeclarationBuilder replacedBuilder,
            [NotNullWhen( true )] out IIntroduceDeclarationTransformation? introduceDeclarationTransformation )
        {
            return this._builderToTransformationMap.TryGetValue( replacedBuilder, out introduceDeclarationTransformation );
        }

        public MemberLevelTransformations GetOrAddMemberLevelTransformations( SyntaxNode declarationSyntax )
        {
            return this._symbolMemberLevelTransformations.GetOrAdd( declarationSyntax, static _ => new MemberLevelTransformations() );
        }

        public MemberLevelTransformations GetOrAddMemberLevelTransformations( IDeclarationBuilder declarationBuilder )
        {
            return this._introductionMemberLevelTransformations.GetOrAdd( declarationBuilder, static _ => new MemberLevelTransformations() );
        }

        public LateTypeLevelTransformations GetOrAddLateTypeLevelTransformations( INamedType type )
        {
            return this._lateTypeLevelTransformations.GetOrAdd( type, static _ => new LateTypeLevelTransformations() );
        }

        internal IReadOnlyList<StatementSyntax> GetInjectedEntryStatements( InjectedMember injectedMember )
        {
            return this.GetInjectedEntryStatements( injectedMember.Declaration, injectedMember );
        }

        internal IReadOnlyList<StatementSyntax> GetInjectedEntryStatements( IMember sourceMember )
        {
            return this.GetInjectedEntryStatements( sourceMember, null );
        }

        internal IReadOnlyList<StatementSyntax> GetInjectedEntryStatements( IDeclaration targetDeclaration, InjectedMember? targetInjectedMember )
        {
            // PERF: Iterating and reversing should be avoided.
            (var canonicalTarget, var statementFilter) =
                targetDeclaration switch
                {
                    IMethod { MethodKind: Code.MethodKind.PropertyGet } =>
                        (targetDeclaration.ContainingDeclaration.AssertNotNull(),
                         static ( InsertedStatement s ) => s.ContextDeclaration is IMethod { MethodKind: Code.MethodKind.PropertyGet }),
                    IMethod { MethodKind: Code.MethodKind.PropertySet } =>
                        (targetDeclaration.ContainingDeclaration.AssertNotNull(),
                         static ( InsertedStatement s ) => s.ContextDeclaration is IMethod { MethodKind: Code.MethodKind.PropertySet }),
                    _ => (targetDeclaration, (Func<InsertedStatement, bool>?) null),
                };

            if ( !this._insertedStatementsByTargetDeclaration.TryGetValue( canonicalTarget, out var insertedStatements ) )
            {
                return ImmutableArray<StatementSyntax>.Empty;
            }

            bool hasInjectedMembers;
            MemberLayerIndex? bottomBound;
            MemberLayerIndex? topBound;

            // If trying to get inserted statements for a source declaration, we need to first find the first injected member.
            if ( !this._injectedMembersByTargetDeclaration.TryGetValue( canonicalTarget, out var injectedMembers ) )
            {
                hasInjectedMembers = false;
                if ( targetInjectedMember == null )
                {
                    bottomBound = null;
                    topBound = null;
                }
                else
                {
                    throw new AssertionFailedException( $"Missing injected members for {targetDeclaration}" );
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
                        throw new AssertionFailedException( $"Missing injected members for {targetDeclaration}" );
                    }

                    bottomBound = GetTransformationMemberLayerIndex( targetInjectedMember.Transformation );
                    topBound =
                        targetInjectedMemberIndex >= injectedMembers.Count - 1
                        ? null
                        : GetTransformationMemberLayerIndex( injectedMembers[targetInjectedMemberIndex + 1].Transformation );
                }
            }

            var statements = new List<StatementSyntax>();

            if ( canonicalTarget is IConstructor )
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
                            && (topBound == null || GetTransformationMemberLayerIndex( s.Transformation ) < topBound)
                            && (statementFilter == null || statementFilter( s )) );

            var orderedInputContractStatements = OrderInputContractStatements( inputContractStatements );

            statements.AddRange(
                orderedInputContractStatements.Select( s =>
                    s.Statement switch
                    {
                        BlockSyntax block => block.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                        _ => s.Statement,
                    } ) );

            return statements;
        }

        internal IReadOnlyList<StatementSyntax> GetInjectedExitStatements( InjectedMember injectedMember )
        {
            return this.GetInjectedExitStatements( injectedMember.Declaration, injectedMember );
        }

        internal IReadOnlyList<StatementSyntax> GetInjectedExitStatements( IDeclaration targetDeclaration, InjectedMember targetInjectedMember )
        {
            // PERF: Iterating and reversing should be avoided.
            (var canonicalTarget, var statementFilter) =
                targetDeclaration switch
                {
                    IMethod { MethodKind: Code.MethodKind.PropertyGet } =>
                        (targetDeclaration.ContainingDeclaration.AssertNotNull(),
                         static ( InsertedStatement s ) => s.ContextDeclaration is IMethod { MethodKind: Code.MethodKind.PropertyGet }),
                    IMethod { MethodKind: Code.MethodKind.PropertySet } =>
                        (targetDeclaration.ContainingDeclaration.AssertNotNull(),
                         static ( InsertedStatement s ) => s.ContextDeclaration is IMethod { MethodKind: Code.MethodKind.PropertySet }),
                    _ => (targetDeclaration, (Func<InsertedStatement, bool>?) null),
                };

            if ( !this._insertedStatementsByTargetDeclaration.TryGetValue( canonicalTarget, out var insertedStatements ) )
            {
                return ImmutableArray<StatementSyntax>.Empty;
            }
            
            MemberLayerIndex? bottomBound;
            MemberLayerIndex? topBound;

            // If trying to get inserted statements for a source declaration, we need to first find the first injected member.
            if ( !this._injectedMembersByTargetDeclaration.TryGetValue( canonicalTarget, out var injectedMembers ) )
            {
                throw new AssertionFailedException( $"Missing injected member for {targetDeclaration} (exit statements are not supported on source members)." );
            }
            else
            {
                if ( targetInjectedMember == null )
                {
                    bottomBound = null;
                    topBound = GetTransformationMemberLayerIndex( injectedMembers.First().Transformation );
                }
                else
                {
                    injectedMembers = injectedMembers.ToOrderedList( x => GetTransformationMemberLayerIndex( x.Transformation ) );

                    var targetInjectedMemberIndex = injectedMembers.IndexOf( targetInjectedMember );

                    if ( targetInjectedMemberIndex < 0 )
                    {
                        throw new AssertionFailedException( $"Missing injected members for {targetDeclaration}" );
                    }

                    bottomBound = GetTransformationMemberLayerIndex( targetInjectedMember.Transformation );
                    topBound =
                        targetInjectedMemberIndex >= injectedMembers.Count - 1
                        ? null
                        : GetTransformationMemberLayerIndex( injectedMembers[targetInjectedMemberIndex + 1].Transformation );
                }
            }

            var statements = new List<StatementSyntax>();

            // For non-initializer statements we have to select a range of statements that fits this injected member.
            var outputContractStatements =
                insertedStatements
                    .Where(
                        s =>
                            s.Kind == InsertedStatementKind.OutputContract
                            && (bottomBound == null || GetTransformationMemberLayerIndex( s.Transformation ) >= bottomBound)
                            && (topBound == null || GetTransformationMemberLayerIndex( s.Transformation ) < topBound)
                            && (statementFilter == null || statementFilter( s )) );

            var orderedOutputContractStatements = OrderOutputContractStatements( outputContractStatements );

            statements.AddRange(
                orderedOutputContractStatements.Select( s =>
                    s.Statement switch
                    {
                        BlockSyntax block => block.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                        _ => s.Statement,
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
        {
            // Makes sure that the order is not changed when override is added in the middle of aspects that insert statements.
            return statements
                .OrderBy( s => s.ContextDeclaration switch
                    {
                        IProperty => 0,
                        IParameter { IsReturnParameter: false } parameter => parameter.Index, // Parameters are checked in order they appear in code.
                        IIndexer indexer => indexer.Parameters.Count, // Indexer value should be checked after parameters.
                        _ => throw new AssertionFailedException( $"Unexpected declaration: '{s.ContextDeclaration}'." )
                    } )
                .ThenByDescending( s => s.Transformation.OrderWithinPipeline )
                .ThenByDescending( s => s.Transformation.OrderWithinPipelineStepAndType )
                .ThenBy( s => s.Transformation.OrderWithinPipelineStepAndTypeAndAspectInstance );
        }

        private static IEnumerable<InsertedStatement> OrderOutputContractStatements( IEnumerable<InsertedStatement> statements )
        {
            // Makes sure that the order is not changed when override is added in the middle of aspects that insert statements.
            return statements
                .OrderBy( s => s.ContextDeclaration switch
                {
                    IProperty => 0,
                    IParameter { IsReturnParameter: false } parameter => parameter.Index, // Parameters are checked in order they appear in code.
                    IIndexer indexer => indexer.Parameters.Count, // Indexer return value should be checked after out/ref parameters.
                    IParameter { IsReturnParameter: true, ContainingDeclaration: IMethod method } => method.Parameters.Count, // Method return value contracts are ordered after
                    _ => throw new AssertionFailedException( $"Unexpected declaration: '{s.ContextDeclaration}'." )
                } )
                .ThenByDescending( s => s.Transformation.OrderWithinPipeline )
                .ThenByDescending( s => s.Transformation.OrderWithinPipelineStepAndType )
                .ThenBy( s => s.Transformation.OrderWithinPipelineStepAndTypeAndAspectInstance );
        }

        private static MemberLayerIndex GetTransformationMemberLayerIndex( ITransformation? transformation )
            => transformation != null
            ? new MemberLayerIndex(
                transformation.OrderWithinPipeline,
                transformation.OrderWithinPipelineStepAndType,
                transformation.OrderWithinPipelineStepAndTypeAndAspectInstance )
            : new MemberLayerIndex(0,0,0);
    }
}