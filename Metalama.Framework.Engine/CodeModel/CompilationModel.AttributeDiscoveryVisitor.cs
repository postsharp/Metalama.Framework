// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    public sealed partial class CompilationModel
    {
        /// <summary>
        /// Discovers custom attributes in a syntax tree and index them by attribute name.
        /// </summary>
        private sealed class AttributeDiscoveryVisitor : SafeSyntaxWalker
        {
            private readonly ImmutableDictionaryOfArray<string, AttributeRef>.Builder _builder =
                ImmutableDictionaryOfArray<string, AttributeRef>.CreateBuilder( StringComparer.Ordinal );

            private readonly CompilationContext _compilationContext;

            public AttributeDiscoveryVisitor( CompilationContext compilationContext )
            {
                this._compilationContext = compilationContext;
            }

            public override void VisitAttribute( AttributeSyntax node )
            {
                // Get the short name of the attribute.
                var name = node.Name switch
                {
                    SimpleNameSyntax simpleName => simpleName.Identifier.Text,
                    QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
                    _ => throw new AssertionFailedException( $"Unexpected node kind {node.Kind()} at '{node.GetLocation()}'." )
                };

                name = name.TrimSuffix( "Attribute" );

                // A local method that adds the attribute.
                void IndexAttribute( SyntaxNode? parentDeclaration, DeclarationRefTargetKind kind )
                {
                    void Add( SyntaxNode? realDeclaration )
                    {
                        this._builder.Add( name, new AttributeRef( node, realDeclaration, kind, this._compilationContext ) );
                    }

                    switch ( parentDeclaration )
                    {
                        case IncompleteMemberSyntax:
                            // This happens at design time when we have an invalid syntax.
                            break;

                        case BaseFieldDeclarationSyntax field:
                            {
                                // In case of fields and field-like events, add the attribute to all defined fields.

                                foreach ( var variable in field.Declaration.Variables )
                                {
                                    Add( variable );
                                }

                                break;
                            }

                        default:
                            Add( parentDeclaration );

                            break;
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
                            this._builder.Add(
                                name,
                                new AttributeRef( node, Ref.FromSymbol( this._compilationContext.Compilation.SourceModule, this._compilationContext ) ) );

                            break;

                        case SyntaxKind.AssemblyKeyword:
                            this._builder.Add(
                                name,
                                new AttributeRef( node, Ref.FromSymbol( this._compilationContext.Compilation.Assembly, this._compilationContext ) ) );

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

            protected override void VisitCore( SyntaxNode? node )
            {
                if ( node is ExpressionSyntax or ExpressionSyntax )
                {
                    // Don't visit any expression or statement deeply. 
                }
                else
                {
                    base.VisitCore( node );
                }
            }

            public ImmutableDictionaryOfArray<string, AttributeRef> GetDiscoveredAttributes() => this._builder.ToImmutable();

            public void Visit( SyntaxTree tree )
            {
                this.Visit( tree.GetRoot() );
            }
        }
    }
}