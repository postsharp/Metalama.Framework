// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.SerializableIds;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Metalama.Framework.Tests.LinkerTests.Runner
{
    // Aspect linker tests' source use [PseudoIntroduction] and [PseudoOverride] attributes, that cause the marked declaration to be removed
    // and transformed into a pseudo-transformation that serves as a linker input. This is mainly to avoid jumping through hoops to reproduce
    // linker-specific problems with the full pipeline.
    //
    // Aspect layer order is collected from proceeds and in the order incidence, if different order is required, one can use attributes on types to force an order:
    // 
    // [LayerOrder(TestAspect1)]
    // [LayerOrder(TestAspect2)]
    // [LayerOrder(TestAspect1, TestLayer)]
    //
    // Pseudo override:
    // 
    // [PseudoOverride(Foo, TestAspect1, TestLayer)]
    // public int Foo_Override(int x, int y)
    // {
    //     return link(this.Foo(x,y));  <--- Points to overridden declaration and is rewritten to an annotated method call.
    // }
    // TODO: annotation order
    //
    // Pseudo introduction - method body should be empty/return default:
    // 
    // [PseudoIntroduction(TestAspect2)]
    // public int Foo(int x, int y)
    // {
    //     return default;
    // }
    //
    // Options:
    //
    // link(this.Foo(x,y), noinline); <--- the target will never be inlined.

    internal sealed partial class LinkerTestInputBuilder
    {
        private readonly ProjectServiceProvider _serviceProvider;
        private readonly TestRewriter _rewriter;

        private readonly List<Func<CompilationModel, TestTransformationBase>> _transformationFactories = new();

        private CompilationModel _initialCompilationModel;
        private Dictionary<string, SyntaxNode> _syntaxNodeMap;

        public LinkerTestInputBuilder( in ProjectServiceProvider serviceProvider, CompilationContext inputCompilationContext )
        {
            this._serviceProvider = serviceProvider;
            this._rewriter = new TestRewriter( serviceProvider, this, inputCompilationContext );
        }

        internal SyntaxNode ProcessSyntaxRoot( SyntaxNode syntaxRoot )
        {
            return this._rewriter.Visit( syntaxRoot )!;
        }

        public AspectLinkerInput ToAspectLinkerInput(CompilationModel initialCompilationModel)
        {
            this._initialCompilationModel = initialCompilationModel;

            this._syntaxNodeMap =
                this._initialCompilationModel.CompilationContext.Compilation.SyntaxTrees.SelectMany( GetNodesWithId )
                .ToDictionary( GetNodeId );

            var orderedLayers = this._rewriter.OrderedAspectLayers
                .Select( ( al, i ) => new OrderedAspectLayer( i, al.AspectName.AssertNotNull(), al.LayerName ) )
                .ToArray();

            var layerOrderLookup = orderedLayers.ToDictionary( x => x.AspectLayerId, x => x.Order );

            var mutableCompilationModel = initialCompilationModel.CreateMutableClone();
            var transformations = new List<ITransformation>();

            foreach (var transformationFactory in this._transformationFactories )
            {
                var transformation = transformationFactory( mutableCompilationModel );

                mutableCompilationModel.AddTransformation( transformation );
                transformations.Add( transformation );
            }

            var linkerInput = new AspectLinkerInput(
                initialCompilationModel,
                mutableCompilationModel.CreateImmutableClone(),
                transformations.ToOrderedList( x => layerOrderLookup[x.AspectLayerId] ),
                orderedLayers,
                null! );

            return linkerInput;
        }

        internal void AddTransformationFactory( Func<CompilationModel, TestTransformationBase> transformationFactoryFunc )
        {
            this._transformationFactories.Add( transformationFactoryFunc );
        }

        internal InsertPosition TranslateInsertPosition( CompilationContext compilationContext, InsertPositionRecord insertPositionRecord )
        {
            switch ( insertPositionRecord )
            {
                case { Relation: var relation, NodeId: { } nodeId }:
                    var node = (MemberDeclarationSyntax) this._syntaxNodeMap[nodeId];
                    return new InsertPosition( relation, node );
                case { Relation: var relation, BuilderData: { } builderData }:
                    return new InsertPosition( relation, builderData );
                default:
                    throw new AssertionFailedException( "Unsupported" );
            }
        }

        internal IFullRef<IDeclaration> TranslateOriginalSymbol( ISymbol overriddenDeclarationSymbol )
        {
            var symbolId = SymbolId.Create( overriddenDeclarationSymbol );
            var durableRef = DurableRefFactory.FromSymbolId<IDeclaration>( symbolId );
            return durableRef.ToFullRef<IDeclaration>( this._initialCompilationModel.RefFactory );
        }
    }
}