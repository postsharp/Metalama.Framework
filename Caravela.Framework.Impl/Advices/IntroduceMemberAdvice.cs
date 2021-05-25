// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
        where TBuilder : MemberOrNamedTypeBuilder
    {
        public IntroductionScope Scope { get; }

        public ConflictBehavior ConflictBehavior { get; }

        public new INamedType TargetDeclaration => (INamedType) base.TargetDeclaration;

        protected TBuilder MemberBuilder { get; init; }

        protected IMemberOrNamedType? TemplateMember { get; }

        public IntroduceMemberAdvice(
            AspectInstance aspect,
            INamedType targetDeclaration,
            IMemberOrNamedType? templateMember,
            IntroductionScope scope,
            ConflictBehavior conflictBehavior,
            string? layerName,
            AdviceOptions? options ) : base( aspect, targetDeclaration, layerName, options )
        {
            this.TemplateMember = templateMember;
            this.Scope = scope;
            this.ConflictBehavior = conflictBehavior;

            // This is to make the nullability analyzer happy. Derived classes are supposed to set this member in the
            // constructor. Other designs are more cumbersome.
            this.MemberBuilder = null!;
        }

        public override void Initialize( IReadOnlyList<Advice>? declarativeAdvices, IDiagnosticAdder diagnosticAdder )
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