// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

namespace Metalama.Framework.Engine.Advising
{
    internal abstract class IntroduceMemberAdvice<TMember, TBuilder> : Advice
        where TMember : class, IMember
        where TBuilder : MemberBuilder
    {
        private readonly Action<TBuilder>? _buildAction;

        public IntroductionScope Scope { get; }

        public OverrideStrategy OverrideStrategy { get; }

        public new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        public TBuilder Builder { get; protected init; }

        protected TemplateMember<TMember>? Template { get; }

        protected string MemberName { get; }

        public IObjectReader Tags { get; }

        public IntroduceMemberAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            string? explicitName,
            TemplateMember<TMember>? template,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            Action<TBuilder>? buildAction,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName )
        {
            var templateAttribute = (ITemplateAttribute?) template?.AdviceAttribute;

            this.MemberName = explicitName ?? templateAttribute?.Name
                ?? template?.Declaration.Name ?? throw new ArgumentNullException( nameof(explicitName) );

            this.Template = template;

            if ( scope != IntroductionScope.Default )
            {
                this.Scope = scope;
            }
            else if ( templateAttribute is IntroduceAttribute introduceAttribute )
            {
                this.Scope = introduceAttribute.Scope;
            }

            this.OverrideStrategy = overrideStrategy;
            this._buildAction = buildAction;
            this.Tags = tags;

            // This is to make the nullability analyzer happy. Derived classes are supposed to set this member in the
            // constructor. Other designs are more cumbersome.
            this.Builder = null!;
        }

        protected virtual void InitializeCore( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder ) { }

        public sealed override void Initialize( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( serviceProvider, diagnosticAdder );

            var templateAttribute = (ITemplateAttribute?) this.Template?.AdviceAttribute;

            this.Builder.Accessibility = this.Template?.Accessibility ?? Accessibility.Private;
            this.Builder.IsSealed = templateAttribute?.IsSealed ?? this.Template?.Declaration.IsSealed ?? false;
            this.Builder.IsVirtual = templateAttribute?.IsVirtual ?? this.Template?.Declaration.IsVirtual ?? false;

            // Handle the introduction scope.
            var targetDeclaration = this.TargetDeclaration.GetTarget( this.SourceCompilation );

            switch ( this.Scope )
            {
                case IntroductionScope.Default:
                    if ( this.Template?.Declaration is { IsStatic: true } || targetDeclaration.IsStatic )
                    {
                        this.Builder.IsStatic = true;
                    }
                    else
                    {
                        this.Builder.IsStatic = false;
                    }

                    break;

                case IntroductionScope.Instance:
                    if ( targetDeclaration.IsStatic )
                    {
                        diagnosticAdder.Report(
                            AdviceDiagnosticDescriptors.CannotIntroduceInstanceMemberIntoStaticType.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration) ) );
                    }

                    this.Builder.IsStatic = false;

                    break;

                case IntroductionScope.Static:
                    this.Builder.IsStatic = true;

                    break;

                case IntroductionScope.Target:
                    this.Builder.IsStatic = targetDeclaration.IsStatic;

                    break;

                default:
                    throw new AssertionFailedException();
            }

            if ( this.Template != null )
            {
                CopyTemplateAttributes( this.Template.Declaration!, this.Builder, serviceProvider );
            }

            this.InitializeCore( serviceProvider, diagnosticAdder );

            this._buildAction?.Invoke( this.Builder );
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

        public override string ToString() => $"Introduce {this.Builder}";
    }
}