﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using System;

namespace Metalama.Framework.Engine.Advices
{
    // ReSharper disable once UnusedType.Global
    // TODO: Use this type and remove the warning waiver.

    internal class IntroduceFieldAdvice : IntroduceMemberAdvice<IField, FieldBuilder>
    {
        public IFieldBuilder Builder => this.MemberBuilder;

        public IntroduceFieldAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            string? explicitName,
            TemplateMember<IField> fieldTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            IObjectReader tags )
            : base( aspect, templateInstance, targetDeclaration,  explicitName, fieldTemplate, scope, overrideStrategy, layerName, tags )
        {
            this.MemberBuilder = new FieldBuilder( this, targetDeclaration, this.MemberName, tags );
            this.MemberBuilder.InitializerTemplate = fieldTemplate.GetInitializerTemplate();
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( diagnosticAdder );

            if ( !this.Template.IsNull )
            {
                this.MemberBuilder.Type = this.Template.Declaration!.Type;
                this.MemberBuilder.Accessibility = this.Template.Declaration!.Accessibility;
                this.MemberBuilder.IsStatic = this.Template.Declaration!.IsStatic;
                this.MemberBuilder.Writeability = this.Template.Declaration!.Writeability;
            }
            else
            {
                this.MemberBuilder.Type = this.SourceCompilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Object );
                this.MemberBuilder.Accessibility = Accessibility.Private;
                this.MemberBuilder.IsStatic = false;
                this.MemberBuilder.Writeability = Writeability.All;
            }
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );
            var existingDeclaration = targetDeclaration.FindClosestUniquelyNamedMember( this.MemberBuilder.Name );

            if ( existingDeclaration != null )
            {
                if ( existingDeclaration is not IField )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration, existingDeclaration.DeclarationKind) ) );
                }

                if ( existingDeclaration.IsStatic != this.MemberBuilder.IsStatic )
                {
                    return
                        AdviceResult.Create(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentStaticity.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                 existingDeclaration.DeclaringType) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceResult.Create(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.MemberBuilder, targetDeclaration,
                                     existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceResult.Create();

                    case OverrideStrategy.New:
                        this.MemberBuilder.IsNew = true;

                        break;

                    case OverrideStrategy.Override:
                        throw new NotSupportedException( "Override is not a supported OverrideStrategy for fields." );

                    default:
                        throw new AssertionFailedException();
                }
            }

            return AdviceResult.Create( this.MemberBuilder );
        }
    }
}