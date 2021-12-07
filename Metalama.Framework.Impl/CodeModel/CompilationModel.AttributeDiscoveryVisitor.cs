// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.CodeModel.References;
using Metalama.Framework.Impl.Collections;
using Metalama.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Impl.CodeModel
{
    internal partial class CompilationModel
    {
        /// <summary>
        /// Discovers custom attributes in a syntax tree and index them by attribute name.
        /// </summary>
        private class AttributeDiscoveryVisitor : CSharpSyntaxWalker
        {
            private readonly ImmutableDictionaryOfArray<string, AttributeRef>.Builder _builder =
                ImmutableDictionaryOfArray<string, AttributeRef>.CreateBuilder( StringComparer.Ordinal );

            private readonly Compilation _compilation;

            public AttributeDiscoveryVisitor( Compilation compilation )
            {
                this._compilation = compilation;
            }

            public override void VisitAttribute( AttributeSyntax node )
            {
                // Get the short name of the attribute.
                var name = node.Name switch
                {
                    SimpleNameSyntax simpleName => simpleName.Identifier.Text,
                    QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
                    _ => throw new AssertionFailedException()
                };

                name = name.TrimEnd( "Attribute" );

                // A local method that adds the attribute.
                void IndexAttribute( SyntaxNode? parentDeclaration, DeclarationRefTargetKind kind )
                {
                    if ( parentDeclaration is BaseFieldDeclarationSyntax field )
                    {
                        // In case of fields and field-like events, add the attribute to all defined fields.

                        foreach ( var variable in field.Declaration.Variables )
                        {
                            this._builder.Add( name, new AttributeRef( node, variable, kind, this._compilation ) );
                        }
                    }
                    else
                    {
                        this._builder.Add( name, new AttributeRef( node, parentDeclaration, kind, this._compilation ) );
                    }
                }

                // Get the parent declaration. 
                var attributeList = (AttributeListSyntax) node.Parent.AssertNotNull();

                var declaration = attributeList.Parent;

                if ( attributeList.Target != null )
                {
                    var targetKind = attributeList.Target.Identifier.Kind();

                    switch ( targetKind )
                    {
                        case SyntaxKind.ModuleKeyword:
                            IndexAttribute( null, DeclarationRefTargetKind.Module );

                            break;

                        case SyntaxKind.AssemblyKeyword:
                            IndexAttribute( null, DeclarationRefTargetKind.Assembly );

                            break;

                        case SyntaxKind.FieldKeyword:
                            IndexAttribute( declaration, DeclarationRefTargetKind.Field );

                            break;

                        case SyntaxKind.ReturnKeyword:
                            IndexAttribute( declaration, DeclarationRefTargetKind.Return );

                            break;

                        case SyntaxKind.ParamKeyword:
                            IndexAttribute( declaration, DeclarationRefTargetKind.Parameter );

                            break;

                        case SyntaxKind.MethodKeyword:
                            if ( declaration is BasePropertyDeclarationSyntax { AccessorList: { } } property )
                            {
                                foreach ( var accessor in property.AccessorList.Accessors )
                                {
                                    IndexAttribute( accessor, DeclarationRefTargetKind.Default );
                                }
                            }

                            break;

                        case SyntaxKind.PropertyKeyword:
                            IndexAttribute( declaration, DeclarationRefTargetKind.Property );

                            break;

                        case SyntaxKind.EventKeyword:
                            IndexAttribute( declaration, DeclarationRefTargetKind.Event );

                            break;

                        default:
                            throw new AssertionFailedException( $"Unexpected attribute target: '{targetKind}'." );
                    }
                }
                else
                {
                    IndexAttribute( declaration, DeclarationRefTargetKind.Default );
                }

                base.VisitAttribute( node );
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node is ExpressionSyntax or ExpressionSyntax )
                {
                    // Don't visit any expression or statement deeply. 
                }
                else
                {
                    base.Visit( node );
                }
            }

            public ImmutableDictionaryOfArray<string, AttributeRef> GetDiscoveredAttributes() => this._builder.ToImmutable();
        }
    }
}