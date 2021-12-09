// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
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
        private class CollectSerializableFieldsVisitor : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly CancellationToken _cancellationToken;
            private readonly List<ISymbol> _serializableFieldsOrProperties;
            private readonly ITypeSymbol _nonSerializedAttribute;

            public IReadOnlyList<ISymbol> SerializableFieldsOrProperties => this._serializableFieldsOrProperties;

            public CollectSerializableFieldsVisitor( SemanticModel semanticModel, ReflectionMapper reflectionMapper, CancellationToken cancellationToken )
            {
                this._semanticModel = semanticModel;
                this._cancellationToken = cancellationToken;
                this._serializableFieldsOrProperties = new List<ISymbol>();
                this._nonSerializedAttribute = reflectionMapper.GetTypeSymbol( typeof(MetaNonSerializedAttribute) );
            }

            public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                foreach ( var declarator in node.Declaration.Variables )
                {
                    var fieldSymbol = this._semanticModel.GetDeclaredSymbol( declarator ).AssertNotNull();

                    if ( !fieldSymbol.GetAttributes().Any( a => SymbolEqualityComparer.Default.Equals( a.AttributeClass, this._nonSerializedAttribute ) ) )
                    {
                        this._serializableFieldsOrProperties.Add( fieldSymbol );
                    }
                }
            }

            public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                var propertySymbol = this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

                if ( !propertySymbol.IsAutoProperty() )
                {
                    return;
                }

                var backingField = propertySymbol.GetBackingField().AssertNotNull();

                if ( !backingField.GetAttributes().Any( a => SymbolEqualityComparer.Default.Equals( a.AttributeClass, this._nonSerializedAttribute ) ) )
                {
                    this._serializableFieldsOrProperties.Add( propertySymbol );
                }
            }
        }
    }
}