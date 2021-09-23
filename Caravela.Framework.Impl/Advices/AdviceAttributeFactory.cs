// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.Advices
{
    internal static class AdviceAttributeFactory
    {
        public static bool TryCreateAdvice<T>(
            this TemplateInfo template,
            AspectInstance aspect,
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
                                        (aspect.AspectClass.DisplayName, templateDeclaration.DeclarationKind, aspectTargetDeclaration.DeclarationKind) ) );

                                advice = null;

                                return false;
                        }

                        switch ( templateDeclaration )
                        {
                            case IMethod templateMethod:
                                var introduceMethodAdvice = new IntroduceMethodAdvice(
                                    aspect,
                                    targetType,
                                    Template.Create( templateMethod, template, TemplateKind.Introduction ),
                                    template.Attribute.Scope,
                                    template.Attribute.WhenExists,
                                    layerName,
                                    null );

                                advice = introduceMethodAdvice;
                                builder = introduceMethodAdvice.Builder;

                                break;

                            case IProperty templateProperty:
                                var propertyTemplate = Template.Create( templateProperty, template, TemplateKind.Introduction );
                                var accessorTemplates = propertyTemplate.GetAccessorTemplates();

                                var introducePropertyAdvice = new IntroducePropertyAdvice(
                                    aspect,
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
                                    targetType,
                                    null,
                                    Template.Create( templateEvent, template, TemplateKind.Introduction ),
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
                                    targetType,
                                    null,
                                    Template.Create( templateField, template, TemplateKind.Introduction ),
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