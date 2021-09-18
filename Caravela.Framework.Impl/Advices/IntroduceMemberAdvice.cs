// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class IntroduceMemberAdvice<TMember, TBuilder> : Advice
        where TMember : class, IMemberOrNamedType
        where TBuilder : MemberOrNamedTypeBuilder
    {
        public IntroductionScope Scope { get; }

        public OverrideStrategy OverrideStrategy { get; }

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        protected TBuilder MemberBuilder { get; init; }

        protected Template<TMember> Template { get; }

        protected TMember? TemplateMember => this.Template.Declaration;

        public IntroduceMemberAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            Template<TMember> template,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, targetDeclaration, layerName, tags )
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
            this.MemberBuilder.Accessibility = this.TemplateMember?.Accessibility ?? Accessibility.Private;

            // Handle the introduction scope.
            switch ( this.Scope )
            {
                case IntroductionScope.Default:
                    if ( this.TemplateMember is { IsStatic: true } || this.TargetDeclaration.IsStatic )
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
                                (this.Aspect.AspectClass.DisplayName, this.MemberBuilder, this.TargetDeclaration) ) );
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

            if ( this.TemplateMember != null )
            {
                CopyAttributes( this.TemplateMember, this.MemberBuilder );
            }
        }

        protected static void CopyAttributes( IDeclaration declaration, IDeclarationBuilder builder )
        {
            // TODO: Don't copy all attributes, but how to decide which ones to keep?
            foreach ( var codeElementAttribute in declaration.Attributes )
            {
                var builderAttribute = builder.AddAttribute(
                    codeElementAttribute.Type,
                    codeElementAttribute.ConstructorArguments.Select( x => x.Value ).ToArray() );

                foreach ( var codeElementAttributeNamedArgument in codeElementAttribute.NamedArguments )
                {
                    builderAttribute.AddNamedArgument( codeElementAttributeNamedArgument.Key, codeElementAttributeNamedArgument.Value.Value );
                }
            }
        }
    }
}