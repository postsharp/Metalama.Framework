// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
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
            private readonly SyntaxNode _typeDeclaration;
            private readonly ISymbolClassifier _symbolClassifier;
            private readonly CancellationToken _cancellationToken;
            private readonly List<ISymbol> _serializableFieldsOrProperties;
            private readonly ITypeSymbol _nonSerializedAttribute;
            private readonly ITypeSymbol _templateAttribute;

            public IReadOnlyList<ISymbol> SerializableFieldsOrProperties => this._serializableFieldsOrProperties;

            public CollectSerializableFieldsVisitor(
                SemanticModel semanticModel,
                SyntaxNode typeDeclaration,
                ReflectionMapper reflectionMapper,
                ISymbolClassifier symbolClassifier,
                CancellationToken cancellationToken )
            {
                this._semanticModel = semanticModel;
                this._typeDeclaration = typeDeclaration;
                this._symbolClassifier = symbolClassifier;
                this._cancellationToken = cancellationToken;
                this._serializableFieldsOrProperties = new List<ISymbol>();
                this._nonSerializedAttribute = reflectionMapper.GetTypeSymbol( typeof(LamaNonSerializedAttribute) );
                this._templateAttribute = reflectionMapper.GetTypeSymbol( typeof(ITemplateAttribute) );
            }

            public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                foreach ( var declarator in node.Declaration.Variables )
                {
                    var fieldSymbol = this._semanticModel.GetDeclaredSymbol( declarator ).AssertNotNull();

                    if ( !fieldSymbol.IsStatic &&
                         !fieldSymbol.GetAttributes()
                             .Any(
                                 a =>
                                     SymbolEqualityComparer.Default.Equals( a.AttributeClass, this._nonSerializedAttribute )
                                     || a.AttributeClass.AssertNotNull().Is( this._templateAttribute ) ) &&
                         this._symbolClassifier.GetTemplateInfo( fieldSymbol ).IsNone )
                    {
                        this._serializableFieldsOrProperties.Add( fieldSymbol );
                    }
                }
            }

            public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                var propertySymbol = this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

                if ( !propertySymbol.IsAutoProperty() || propertySymbol.IsStatic )
                {
                    return;
                }

                var backingField = propertySymbol.GetBackingField().AssertNotNull();

                if ( !backingField.GetAttributes().Any( a => SymbolEqualityComparer.Default.Equals( a.AttributeClass, this._nonSerializedAttribute ) )
                     && !propertySymbol.GetAttributes()
                         .Any(
                             a =>
                                 SymbolEqualityComparer.Default.Equals( a.AttributeClass, this._nonSerializedAttribute )
                                 || a.AttributeClass.AssertNotNull().Is( this._templateAttribute ) )
                     && this._symbolClassifier.GetTemplateInfo( propertySymbol ).IsNone )
                {
                    this._serializableFieldsOrProperties.Add( propertySymbol );
                }
            }

            public override void VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                if ( this._typeDeclaration != node )
                {
                    return;
                }

                base.VisitClassDeclaration( node );
            }

            public override void VisitStructDeclaration( StructDeclarationSyntax node )
            {
                if ( this._typeDeclaration != node )
                {
                    return;
                }

                base.VisitStructDeclaration( node );
            }

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                if ( this._typeDeclaration != node )
                {
                    return;
                }

                base.VisitRecordDeclaration( node );
            }
        }
    }
}