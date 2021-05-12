// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal class OverriddenProperty : INonObservableTransformation, IMemberIntroduction, IOverriddenElement
    {
        public Advice Advice { get; }

        ICodeElement IOverriddenElement.OverriddenElement => this.OverriddenDeclaration;

        public IProperty OverriddenDeclaration { get; }

        public IProperty? TemplateProperty { get; }

        public IMethod? GetTemplateMethod { get; }

        public IMethod? SetTemplateMethod { get; }

        public AspectLinkerOptions? LinkerOptions { get; }

        public OverriddenProperty(
            Advice advice,
            IProperty overriddenDeclaration,
            IProperty? templateProperty,
            IMethod? getTemplateMethod,
            IMethod? setTemplateMethod,
            AspectLinkerOptions? linkerOptions = null )
        {
            Invariant.Assert( advice != null );
            Invariant.Assert( overriddenDeclaration != null );

            // We need either property template or (one or more) accessor templates, but never both.
            Invariant.Assert( templateProperty != null || getTemplateMethod != null || setTemplateMethod != null );
            Invariant.Assert( !((templateProperty != null) && (getTemplateMethod != null || setTemplateMethod != null)) );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
            this.TemplateProperty = templateProperty;
            this.GetTemplateMethod = getTemplateMethod;
            this.SetTemplateMethod = setTemplateMethod;
            this.LinkerOptions = linkerOptions;
        }

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree
            => this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
                ? introduction.TargetSyntaxTree
                : ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.DeclaringSyntaxReferences.First().SyntaxTree;

        public IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            using ( context.DiagnosticSink.WithDefaultScope( this.OverriddenDeclaration ) )
            {
                var propertyName = context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration );

                string? compiledGetTemplateName;
                string? compiledSetTemplateName;

                if ( this.TemplateProperty != null )
                {
                    compiledGetTemplateName = TemplateNameHelper.GetCompiledPropertyGetTemplateName( this.TemplateProperty.Name );
                    compiledSetTemplateName = TemplateNameHelper.GetCompiledPropertySetTemplateName( this.TemplateProperty.Name );
                }
                else
                {
                    compiledGetTemplateName = this.GetTemplateMethod != null ? TemplateNameHelper.GetCompiledTemplateName( this.GetTemplateMethod.Name ) : null;
                    compiledSetTemplateName = this.SetTemplateMethod != null ? TemplateNameHelper.GetCompiledTemplateName( this.SetTemplateMethod.Name ) : null;
                }

                var getTemplateMethod = compiledGetTemplateName != null ? this.Advice.Aspect.GetTemplateMethod( compiledGetTemplateName ) : null;
                var setTemplateMethod = compiledSetTemplateName != null ? this.Advice.Aspect.GetTemplateMethod( compiledSetTemplateName ) : null;

                var getAccessorBody = getTemplateMethod != null && this.OverriddenDeclaration.Getter != null
                    ? this.ExpandAccessorTemplate( context, getTemplateMethod, this.OverriddenDeclaration.Getter )
                    : null;

                var setAccessorBody = setTemplateMethod != null && this.OverriddenDeclaration.Setter != null
                    ? this.ExpandAccessorTemplate( context, setTemplateMethod, this.OverriddenDeclaration.Setter )
                    : null;

                var overrides = new[]
                {
                    new IntroducedMember(
                        this,
                        PropertyDeclaration(
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.GetSyntaxModifierList(),
                            this.OverriddenDeclaration.GetSyntaxReturnType(),
                            null,
                            Identifier( propertyName ),
                            AccessorList(
                                List(
                                    new[]
                                        {
                                            getAccessorBody != null
                                                ? AccessorDeclaration(
                                                    SyntaxKind.GetAccessorDeclaration,
                                                    List<AttributeListSyntax>(),
                                                    this.OverriddenDeclaration.Getter.AssertNotNull().GetSyntaxModifierList(),
                                                    getAccessorBody )
                                                : null,
                                            setAccessorBody != null
                                                ? AccessorDeclaration(
                                                    SyntaxKind.SetAccessorDeclaration,
                                                    List<AttributeListSyntax>(),
                                                    this.OverriddenDeclaration.Setter.AssertNotNull().GetSyntaxModifierList(),
                                                    setAccessorBody )
                                                : null
                                        }.Where( a => a != null )
                                        .AssertNoneNull() ) ),
                            null,
                            null ),
                        this.Advice.AspectLayerId,
                        IntroducedMemberSemantic.PropertyOverride,
                        this.LinkerOptions,
                        this.OverriddenDeclaration )
                };

                return overrides;
            }
        }

        private BlockSyntax ExpandAccessorTemplate( in MemberIntroductionContext context, MethodInfo templateMethod, IMethod accessor )
        {
            using ( context.DiagnosticSink.WithDefaultScope( accessor ) )
            {
                var expansionContext = new TemplateExpansionContext(
                    this.Advice.Aspect.Aspect,
                    accessor,
                    this.OverriddenDeclaration.Compilation,
                    new LinkerOverridePropertyProceedImpl(
                        this.Advice.AspectLayerId,
                        accessor,
                        LinkerAnnotationOrder.Default,
                        context.SyntaxFactory ),
                    context.LexicalScope,
                    context.DiagnosticSink );

                return new TemplateDriver( templateMethod ).ExpandDeclaration( expansionContext );
            }
        }

        public MemberDeclarationSyntax InsertPositionNode
        {
            get
            {
                // TODO: Select a good syntax reference if there are multiple (partial class, partial method).
                var propertySymbol = (this.OverriddenDeclaration as Property)?.Symbol;

                if ( propertySymbol != null )
                {
                    return propertySymbol.DeclaringSyntaxReferences.Select( x => (PropertyDeclarationSyntax) x.GetSyntax() ).First();
                }

                var typeSymbol = ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol;

                return typeSymbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).First();
            }
        }
    }
}