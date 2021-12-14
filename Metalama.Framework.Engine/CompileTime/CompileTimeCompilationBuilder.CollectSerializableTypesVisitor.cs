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

            public IReadOnlyList<SerializableTypeInfo> SerializableTypes => this._serializableTypes;

            public CollectSerializableTypesVisitor( SemanticModel semanticModel, ReflectionMapper reflectionMapper, CancellationToken cancellationToken )
            {
                this._semanticModel = semanticModel;
                this._reflectionMapper = reflectionMapper;
                this._cancellationToken = cancellationToken;
                this._serializableTypes = new List<SerializableTypeInfo>();
            }

            private void ProcessTypeDeclaration( SyntaxNode node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                var declaredSymbol = (INamedTypeSymbol) this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

                var serializableInterface = this._reflectionMapper.GetTypeSymbol( typeof(ILamaSerializable) );

                if ( !declaredSymbol.AllInterfaces.Any( i => SymbolEqualityComparer.Default.Equals( i, serializableInterface ) ) )
                {
                    return;
                }

                var innerVisitor = new CollectSerializableFieldsVisitor( this._semanticModel, node, this._reflectionMapper, this._cancellationToken );

                innerVisitor.Visit( node );

                this._serializableTypes.Add( new SerializableTypeInfo( declaredSymbol, innerVisitor.SerializableFieldsOrProperties ) );
            }

            public override void VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                base.VisitClassDeclaration( node );
                this.ProcessTypeDeclaration( node );
            }

            public override void VisitStructDeclaration( StructDeclarationSyntax node )
            {
                base.VisitStructDeclaration( node );
                this.ProcessTypeDeclaration( node );
            }

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                base.VisitRecordDeclaration( node );
                this.ProcessTypeDeclaration( node );
            }
        }
    }
}