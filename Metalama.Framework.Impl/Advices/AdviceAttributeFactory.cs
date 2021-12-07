﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.CodeModel.Builders;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Impl.Advices
{
    internal static class AdviceAttributeFactory
    {
        public static bool TryCreateAdvice<T>(
            this TemplateInfo template,
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IDiagnosticAdder diagnosticAdder,
            T aspectTargetDeclaration,
            IDeclaration templateDeclaration,
            string? layerName,
            [NotNullWhen( true )] out Advice? advice )
            where T : IDeclaration
        {
            switch ( template.AttributeType )
            {
                case TemplateAttributeType.Introduction:
                    {
                        IMemberBuilder builder;
                        INamedType targetType;

                        switch ( aspectTargetDeclaration )
                        {
                            case IMember member:
                                targetType = member.DeclaringType;

                                break;

                            case INamedType type:
                                targetType = type;

                                break;

                            default:
                                diagnosticAdder.Report(
                                    AdviceDiagnosticDescriptors.CannotUseIntroduceWithoutDeclaringType.CreateDiagnostic(
                                        aspectTargetDeclaration.GetDiagnosticLocation(),
                                        (aspect.AspectClass.ShortName, templateDeclaration.DeclarationKind, aspectTargetDeclaration.DeclarationKind) ) );

                                advice = null;

                                return false;
                        }

                        switch ( templateDeclaration )
                        {
                            case IMethod templateMethod:
                                var introduceMethodAdvice = new IntroduceMethodAdvice(
                                    aspect,
                                    templateInstance,
                                    targetType,
                                    TemplateMember.Create( templateMethod, template, TemplateKind.Introduction ),
                                    template.Attribute.Scope,
                                    template.Attribute.WhenExists,
                                    layerName,
                                    null );

                                advice = introduceMethodAdvice;
                                builder = introduceMethodAdvice.Builder;

                                break;

                            case IProperty templateProperty:
                                var propertyTemplate = TemplateMember.Create( templateProperty, template, TemplateKind.Introduction );
                                var accessorTemplates = propertyTemplate.GetAccessorTemplates();

                                var introducePropertyAdvice = new IntroducePropertyAdvice(
                                    aspect,
                                    templateInstance,
                                    targetType,
                                    null,
                                    propertyTemplate,
                                    accessorTemplates.Get,
                                    accessorTemplates.Set,
                                    template.Attribute.Scope,
                                    template.Attribute.WhenExists,
                                    layerName,
                                    null );

                                advice = introducePropertyAdvice;
                                builder = introducePropertyAdvice.Builder;

                                break;

                            case IEvent templateEvent:
                                var introduceEventAdvice = new IntroduceEventAdvice(
                                    aspect,
                                    templateInstance,
                                    targetType,
                                    null,
                                    TemplateMember.Create( templateEvent, template, TemplateKind.Introduction ),
                                    default,
                                    default,
                                    template.Attribute.Scope,
                                    template.Attribute.WhenExists,
                                    layerName,
                                    null );

                                advice = introduceEventAdvice;
                                builder = introduceEventAdvice.Builder;

                                break;

                            case IField templateField:
                                var introduceFieldAdvice = new IntroduceFieldAdvice(
                                    aspect,
                                    templateInstance,
                                    targetType,
                                    null,
                                    TemplateMember.Create( templateField, template, TemplateKind.Introduction ),
                                    template.Attribute.Scope,
                                    template.Attribute.WhenExists,
                                    layerName );

                                advice = introduceFieldAdvice;
                                builder = introduceFieldAdvice.Builder;

                                break;

                            default:
                                throw new AssertionFailedException( $"Don't know how to introduce a {templateDeclaration.DeclarationKind}." );
                        }

                        advice.Initialize( Array.Empty<Advice>(), diagnosticAdder );

                        ((MemberBuilder) builder).ApplyTemplateAttribute( template.Attribute );

                        return true;
                    }
            }

            throw new NotImplementedException( $"No implementation for advice attribute {template.AttributeType}." );
        }
    }
}