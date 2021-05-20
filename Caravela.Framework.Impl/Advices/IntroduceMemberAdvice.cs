// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class IntroduceMemberAdvice<TBuilder> : Advice
        where TBuilder : MemberBuilder
    {
        public IntroductionScope Scope { get; }

        public ConflictBehavior ConflictBehavior { get; }

        // Null is for types.
        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        public AspectLinkerOptions? LinkerOptions { get; }

        protected abstract TBuilder MemberBuilder { get; set; }

        protected IMember? TemplateMember { get; }

        public IntroduceMemberAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IMember? templateMember,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            IReadOnlyDictionary<string, object?> tags,
            AspectLinkerOptions? linkerOptions ) : base( aspect, targetDeclaration, tags )
        {
            this.TemplateMember = templateMember;
            this.Scope = scope;
            this.ConflictBehavior = conflictBehavior;
            this.LinkerOptions = linkerOptions;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            this.MemberBuilder.Accessibility = this.TemplateMember != null ? this.TemplateMember.Accessibility : Accessibility.Private;

            // Handle the introduction scope.
            switch ( this.Scope )
            {
                case IntroductionScope.Default:
                    if ( this.TemplateMember != null && this.TemplateMember.IsStatic )
                    {
                        goto case IntroductionScope.Static;
                    }
                    else
                    {
                        if ( this.TargetDeclaration != null )
                        {
                            goto case IntroductionScope.Target;
                        }
                        else
                        {
                            goto case IntroductionScope.Instance;
                        }
                    }

                case IntroductionScope.Instance:
                    if ( this.TargetDeclaration is IType && this.TargetDeclaration.IsStatic )
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
                    this.MemberBuilder.IsStatic = this.TargetDeclaration.AssertNotNull().IsStatic;

                    break;

                default:
                    throw new AssertionFailedException();
            }

            if ( this.TemplateMember != null )
            {
                CopyAttributes( this.TemplateMember, this.MemberBuilder );
            }
        }

        protected static void CopyAttributes( ICodeElement codeElement, ICodeElementBuilder builder )
        {
            // TODO: Don't copy all attributes, but how to decide which ones to keep?
            foreach ( var codeElementAttribute in codeElement.Attributes )
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