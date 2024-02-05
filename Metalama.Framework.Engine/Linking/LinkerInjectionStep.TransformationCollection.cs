﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        private readonly ConcurrentDictionary<IMember, List<(InsertedStatement InsertedStatement, InjectedMember? LatestInjectedMember)>> _insertedStatementsByTargetDeclaration;
        private readonly ConcurrentDictionary<IDeclaration, List<InjectedMember>> _injectedMembersByTargetDeclaration;
        private readonly ConcurrentDictionary<IDeclaration, IReadOnlyList<IntroduceParameterTransformation>> _introducedParametersByTargetDeclaration;

        public IReadOnlyCollection<InjectedMember> InjectedMembers => this._injectedMembers;

        public IReadOnlyDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> BuilderToTransformationMap => this._builderToTransformationMap;

        public IReadOnlyDictionary<IDeclaration, IReadOnlyList<IntroduceParameterTransformation>> IntroducedParametersByTargetDeclaration => this._introducedParametersByTargetDeclaration;

        public TransformationCollection( TransformationLinkerOrderComparer comparer )
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
            this._insertedStatementsByTargetDeclaration = new();
            this._injectedMembersByTargetDeclaration = new();
            this._introducedParametersByTargetDeclaration = new();
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

                var declarationInjectedMembers = this._injectedMembersByTargetDeclaration.GetOrAdd( injectedMember.Declaration, _ => new List<InjectedMember>() );

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

        public void AddInsertedStatements( IMember targetMember, IEnumerable<InsertedStatement> statements )
        {
            var statementList = this._insertedStatementsByTargetDeclaration.GetOrAdd( targetMember, _ => new() );

            InjectedMember? lastInjectedMember = null;
            
            if (this._injectedMembersByTargetDeclaration.TryGetValue(targetMember, out var injectedMemberList ) )
            {
                lock ( injectedMemberList )
                {
                    lastInjectedMember = injectedMemberList[^1];
                }
            }

            lock (statementList)
            {
                statementList.AddRange( statements.Select( x => (x, lastInjectedMember) ) );
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
            lock (this._nodesWithModifiedAttributes)
            {
                this._nodesWithModifiedAttributes.Add( node );
            }
        }

        internal void AddIntroducedParameter( IntroduceParameterTransformation introduceParameterTransformation )
        {
            var parameterList = this._introducedParametersByTargetDeclaration.GetOrAdd( introduceParameterTransformation.TargetDeclaration, _ => new List<IntroduceParameterTransformation>() );

            lock ( parameterList )
            {
                ((List<IntroduceParameterTransformation>)parameterList).Add( introduceParameterTransformation );
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

        public bool TryGetMemberLevelTransformations( SyntaxNode node, [NotNullWhen(true)] out MemberLevelTransformations? memberLevelTransformations )
        {
            return this._symbolMemberLevelTransformations.TryGetValue(node, out memberLevelTransformations);
        }

        public bool TryGetMemberLevelTransformations( IDeclarationBuilder builder, [NotNullWhen( true )] out MemberLevelTransformations? memberLevelTransformations )
        {
            return this._introductionMemberLevelTransformations.TryGetValue( builder, out memberLevelTransformations );
        }

        public bool HasMemberLevelTransformations( SyntaxNode syntax )
        {
            return this._symbolMemberLevelTransformations.ContainsKey( syntax );
        }

        public async Task FinalizeAsync(TransformationLinkerOrderComparer transformationComparer, IConcurrentTaskRunner concurrentTaskRunner, CancellationToken cancellationToken)
        {
            await concurrentTaskRunner.RunInParallelAsync(
                this._introductionMemberLevelTransformations.Values,
                t => t.Sort( transformationComparer ),
                cancellationToken );

            await concurrentTaskRunner.RunInParallelAsync(
                this._symbolMemberLevelTransformations.Values,
                t => t.Sort( transformationComparer ),
                cancellationToken );
        }

        public void AddIntroduceTransformation( IDeclarationBuilder declarationBuilder, IIntroduceDeclarationTransformation introduceDeclarationTransformation )
        {
            var wasAdded = this._builderToTransformationMap.TryAdd( declarationBuilder, introduceDeclarationTransformation );

            Invariant.Assert( wasAdded );
        }

        public bool TryGetIntroduceDeclarationTransformation( IDeclarationBuilder replacedBuilder, [NotNullWhen( true )] out IIntroduceDeclarationTransformation? introduceDeclarationTransformation )
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

        internal IReadOnlyList<StatementSyntax> GetInjectedInitializerStatements( InjectedMember injectedMember )
        {
            if ( injectedMember.Declaration is not IMember member
                 || !this._insertedStatementsByTargetDeclaration.TryGetValue( member, out var insertedStatements ) )
            {
                return ImmutableArray<StatementSyntax>.Empty;
            }

            var statements = new List<StatementSyntax>();

            if ( this._injectedMembersByTargetDeclaration.TryGetValue(member, out var injectedMembers ) 
                 && injectedMembers[^1] == injectedMember )
            {
                var initializerStatements = 
                    insertedStatements
                    .Where( s => s.InsertedStatement.Kind == InsertedStatementKind.Initializer )
                    .Select( s => s.InsertedStatement );

                var orderedInitializerStatements = Order( initializerStatements );

                // This is the last override, insert all Initializer statements.
                statements.AddRange( orderedInitializerStatements.Select( s => s.Statement ) );
            }

            return statements;

            static IEnumerable<InsertedStatement> Order( IEnumerable<InsertedStatement> statements )
                => statements
                    .OrderBy(
                        s => s.ContextDeclaration switch
                        {
                            IMember => 0,
                            INamedType => 1,
                            _ => throw new AssertionFailedException( $"Unexpected declaration: '{s.ContextDeclaration}'." )
                        } )
                    .ThenBy( s => (s.ContextDeclaration as IMember)?.ToDisplayString() );
        }

        internal IReadOnlyList<StatementSyntax> GetInjectedEntryStatements( IMember sourceMember )
        {
            if ( !this._insertedStatementsByTargetDeclaration.TryGetValue( sourceMember, out var insertedStatements ) )
            {
                return ImmutableArray<StatementSyntax>.Empty;
            }

            var statements = new List<StatementSyntax>();

            if ( !this._injectedMembersByTargetDeclaration.TryGetValue( sourceMember, out var injectedMembers )
                || injectedMembers.Count == 0)
            {
                // This is the last override, insert all Initializer statements.
                statements.AddRange( insertedStatements.Where( s => s.InsertedStatement.Kind == InsertedStatementKind.Initializer ).Select( s => s.InsertedStatement.Statement ) );
            }

            return statements;
        }
    }
}