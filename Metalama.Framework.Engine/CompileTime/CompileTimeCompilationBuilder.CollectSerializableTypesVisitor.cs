// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        /// <summary>
        /// Determines if a syntax tree has compile-time code. The result is exposed in the <see cref="SerializableTypes"/> property.
        /// </summary>
        private class CollectSerializableTypesVisitor : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly ReflectionMapper _reflectionMapper;
            private readonly CancellationToken _cancellationToken;
            private readonly List<SerializableTypeInfo> _serializableTypes;
            private readonly ISymbolClassifier _symbolClassifier;

            public IReadOnlyList<SerializableTypeInfo> SerializableTypes => this._serializableTypes;

            public CollectSerializableTypesVisitor(
                SemanticModel semanticModel,
                ReflectionMapper reflectionMapper,
                ISymbolClassifier symbolClassifier,
                CancellationToken cancellationToken )
            {
                this._semanticModel = semanticModel;
                this._reflectionMapper = reflectionMapper;
                this._cancellationToken = cancellationToken;
                this._symbolClassifier = symbolClassifier;
                this._serializableTypes = new List<SerializableTypeInfo>();
            }

            private void VisitTypeDeclaration( SyntaxNode node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                var declaredSymbol = (INamedTypeSymbol) this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

                var serializableInterface = this._reflectionMapper.GetTypeSymbol( typeof(ILamaSerializable) );

                if ( !declaredSymbol.AllInterfaces.Any( i => SymbolEqualityComparer.Default.Equals( i, serializableInterface ) ) )
                {
                    return;
                }

                var innerVisitor = new CollectSerializableFieldsVisitor(
                    this._semanticModel,
                    this._reflectionMapper,
                    this._symbolClassifier,
                    this._cancellationToken );

                innerVisitor.Visit( node );

                this._serializableTypes.Add( new SerializableTypeInfo( declaredSymbol, innerVisitor.SerializableFieldsOrProperties ) );
            }

            public override void VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                this.VisitTypeDeclaration( node );
                base.VisitClassDeclaration( node );
            }

            public override void VisitStructDeclaration( StructDeclarationSyntax node )
            {
                this.VisitTypeDeclaration( node );
                base.VisitStructDeclaration( node );
            }

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                this.VisitTypeDeclaration( node );
                base.VisitRecordDeclaration( node );
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node ) { }

            public override void VisitFieldDeclaration( FieldDeclarationSyntax node ) { }

            public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node ) { }

            public override void VisitPropertyPatternClause( PropertyPatternClauseSyntax node ) { }

            public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node ) { }

            public override void VisitConstructorDeclaration( ConstructorDeclarationSyntax node ) { }

            public override void VisitIndexerDeclaration( IndexerDeclarationSyntax node ) { }

            public override void VisitOperatorDeclaration( OperatorDeclarationSyntax node ) { }

            public override void VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node ) { }

            public override void VisitEventDeclaration( EventDeclarationSyntax node ) { }

            public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node ) { }
        }
    }
}