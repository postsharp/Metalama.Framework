// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Project;
using System;

namespace Metalama.Framework.Engine.Advices
{
    internal abstract class IntroduceMemberAdvice<TMember, TBuilder> : Advice, IIntroductionAdvice
        where TMember : class, IMember
        where TBuilder : MemberBuilder
    {
        public IntroductionScope Scope { get; }

        public OverrideStrategy OverrideStrategy { get; }

        public new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        protected TBuilder MemberBuilder { get; init; }

        IDeclarationBuilder IIntroductionAdvice.Builder => this.MemberBuilder;

        protected TemplateMember<TMember> Template { get; }

        protected string MemberName { get; }

        public IObjectReader Tags { get; }

        public IntroduceMemberAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            string? explicitName,
            TemplateMember<TMember> template,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, layerName )
        {
            this.MemberName = explicitName ?? template.TemplateAttribute?.Name
                ?? template.Declaration?.Name ?? throw new ArgumentNullException( nameof(explicitName) );

            this.Template = template;

            if ( scope != IntroductionScope.Default )
            {
                this.Scope = scope;
            }
            else if ( template.TemplateAttribute is IntroduceAttribute introduceAttribute )
            {
                this.Scope = introduceAttribute.Scope;
            }

            this.OverrideStrategy = overrideStrategy;
            this.Tags = tags;

            // This is to make the nullability analyzer happy. Derived classes are supposed to set this member in the
            // constructor. Other designs are more cumbersome.
            this.MemberBuilder = null!;
        }

        public override void Initialize( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            var templateAttribute = this.Template.TemplateAttribute;

            this.MemberBuilder.Accessibility = templateAttribute?.GetAccessibility() ?? this.Template.Declaration?.Accessibility ?? Accessibility.Private;
            this.MemberBuilder.IsSealed = templateAttribute?.GetIsSealed() ?? this.Template.Declaration?.IsSealed ?? false;
            this.MemberBuilder.IsVirtual = templateAttribute?.GetIsVirtual() ?? this.Template.Declaration?.IsVirtual ?? false;

            // Handle the introduction scope.
            var targetDeclaration = this.TargetDeclaration.GetTarget( this.SourceCompilation );

            switch ( this.Scope )
            {
                case IntroductionScope.Default:
                    if ( this.Template.Declaration is { IsStatic: true } || targetDeclaration.IsStatic )
                    {
                        this.MemberBuilder.IsStatic = true;
                    }
                    else
                    {
                        this.MemberBuilder.IsStatic = false;
                    }

                    break;

                case IntroductionScope.Instance:
                    if ( targetDeclaration.IsStatic )
                    {
                        // Diagnostics are reported to a sink when the advice is declarative, but as an exception when it is programmatic. 
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.CannotIntroduceInstanceMemberIntoStaticType.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration) ) );
                    }

                    this.MemberBuilder.IsStatic = false;

                    break;

                case IntroductionScope.Static:
                    this.MemberBuilder.IsStatic = true;

                    break;

                case IntroductionScope.Target:
                    this.MemberBuilder.IsStatic = targetDeclaration.IsStatic;

                    break;

                default:
                    throw new AssertionFailedException();
            }

            if ( this.Template.Declaration != null )
            {
                CopyTemplateAttributes( this.Template.Declaration, this.MemberBuilder, serviceProvider );
            }
        }

        protected static void CopyTemplateAttributes( IDeclaration declaration, IDeclarationBuilder builder, IServiceProvider serviceProvider )
        {
            var classificationService = serviceProvider.GetRequiredService<AttributeClassificationService>();

            foreach ( var codeElementAttribute in declaration.Attributes )
            {
                if ( classificationService.MustCopyTemplateAttribute( codeElementAttribute ) )
                {
                    builder.AddAttribute( codeElementAttribute.ToAttributeConstruction() );
                }
            }
        }

        public override string ToString() => $"Introduce {this.MemberBuilder}";
    }
}