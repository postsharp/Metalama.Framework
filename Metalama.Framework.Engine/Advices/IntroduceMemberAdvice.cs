// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advices
{
    internal abstract class IntroduceMemberAdvice<TMember, TBuilder> : Advice
        where TMember : class, IMemberOrNamedType
        where TBuilder : MemberOrNamedTypeBuilder
    {
        public IntroductionScope Scope { get; }

        public OverrideStrategy OverrideStrategy { get; }

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        protected TBuilder MemberBuilder { get; init; }

        protected TemplateMember<TMember> Template { get; }

        public IntroduceMemberAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            TemplateMember<TMember> template,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.Template = template;
            this.Scope = scope;
            this.OverrideStrategy = overrideStrategy;

            // This is to make the nullability analyzer happy. Derived classes are supposed to set this member in the
            // constructor. Other designs are more cumbersome.
            this.MemberBuilder = null!;
        }

        public override void Initialize( IReadOnlyList<Advice> declarativeAdvices, IDiagnosticAdder diagnosticAdder )
        {
            this.MemberBuilder.Accessibility = this.Template.Declaration?.Accessibility ?? Accessibility.Private;

            // Handle the introduction scope.
            switch ( this.Scope )
            {
                case IntroductionScope.Default:
                    if ( this.Template.Declaration is { IsStatic: true } || this.TargetDeclaration.IsStatic )
                    {
                        this.MemberBuilder.IsStatic = true;
                    }
                    else
                    {
                        this.MemberBuilder.IsStatic = false;
                    }

                    break;

                case IntroductionScope.Instance:
                    if ( this.TargetDeclaration.IsStatic )
                    {
                        // Diagnostics are reported to a sink when the advice is declarative, but as an exception when it is programmatic. 
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.CannotIntroduceInstanceMemberIntoStaticType.CreateDiagnostic(
                                this.TargetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.MemberBuilder, this.TargetDeclaration) ) );
                    }

                    this.MemberBuilder.IsStatic = false;

                    break;

                case IntroductionScope.Static:
                    this.MemberBuilder.IsStatic = true;

                    break;

                case IntroductionScope.Target:
                    this.MemberBuilder.IsStatic = this.TargetDeclaration.IsStatic;

                    break;

                default:
                    throw new AssertionFailedException();
            }

            if ( this.Template.Declaration != null )
            {
                CopyAttributes( this.Template.Declaration, this.MemberBuilder );
            }
        }

        protected static void CopyAttributes( IDeclaration declaration, IDeclarationBuilder builder )
        {
            // TODO: Don't copy all attributes, but how to decide which ones to keep?
            foreach ( var codeElementAttribute in declaration.Attributes )
            {
                builder.AddAttribute( codeElementAttribute.ToAttributeConstruction() );
            }
        }
    }
}