// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal abstract class IntroduceMemberAdvice<TMember, TBuilder> : IntroduceMemberOrNamedTypeAdvice<TMember, TBuilder>
        where TMember : class, IMember
        where TBuilder : MemberBuilder
    {
        private readonly IntroductionScope _scope;

        protected OverrideStrategy OverrideStrategy { get; }

        protected new Ref<INamedType> TargetDeclaration => base.TargetDeclaration.As<INamedType>();

        protected TemplateMember<TMember>? Template { get; }

        protected string MemberName { get; }

        protected IObjectReader Tags { get; }

        protected IntroduceMemberAdvice(
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
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, buildAction, layerName )
        {
            var templateAttribute = (ITemplateAttribute?) template?.AdviceAttribute;
            var templateAttributeProperties = templateAttribute?.Properties;

            this.MemberName = explicitName ?? templateAttributeProperties?.Name
                ?? template?.Declaration.Name ?? throw new ArgumentNullException( nameof(explicitName) );

            this.Template = template;

            if ( scope != IntroductionScope.Default )
            {
                this._scope = scope;
            }
            else if ( templateAttribute is IntroduceAttribute introduceAttribute )
            {
                this._scope = introduceAttribute.Scope;
            }

            this.OverrideStrategy = overrideStrategy;
            this.Tags = tags;
        }

        protected virtual void InitializeCore(
            ProjectServiceProvider serviceProvider,
            IDiagnosticAdder diagnosticAdder,
            TemplateAttributeProperties? templateAttributeProperties ) { }

        public sealed override void Initialize( ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( serviceProvider, diagnosticAdder );

            var templateAttribute = (ITemplateAttribute?) this.Template?.AdviceAttribute;
            var templateAttributeProperties = templateAttribute?.Properties;

            this.Builder.Accessibility = this.Template?.Accessibility ?? Accessibility.Private;
            this.Builder.IsSealed = templateAttributeProperties?.IsSealed ?? this.Template?.Declaration.IsSealed ?? false;
            this.Builder.IsVirtual = templateAttributeProperties?.IsVirtual ?? this.Template?.Declaration.IsVirtual ?? false;

            // Handle the introduction scope.
            var targetDeclaration = this.TargetDeclaration.GetTarget( this.SourceCompilation );

            switch ( this._scope )
            {
                case IntroductionScope.Default:
                    if ( this.Template?.Declaration is { IsStatic: true } )
                    {
                        this.Builder.IsStatic = true;
                    }
                    else
                    {
                        this.Builder.IsStatic = false;
                    }

                    break;

                case IntroductionScope.Instance:
                    this.Builder.IsStatic = false;

                    break;

                case IntroductionScope.Static:
                    this.Builder.IsStatic = true;

                    break;

                case IntroductionScope.Target:
                    this.Builder.IsStatic = targetDeclaration.IsStatic;

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected IntroductionScope: {this._scope}." );
            }

            if ( this.Template != null )
            {
                CopyTemplateAttributes( this.Template.Declaration!, this.Builder, serviceProvider );
            }

            this.InitializeCore( serviceProvider, diagnosticAdder, templateAttributeProperties );

            this.BuildAction?.Invoke( this.Builder );

            this.ValidateBuilder( targetDeclaration, diagnosticAdder );
        }

        protected virtual void ValidateBuilder( INamedType targetDeclaration, IDiagnosticAdder diagnosticAdder )
        {
            // Check that static member is not virtual.
            if ( this.Builder is { IsStatic: true, IsVirtual: true } )
            {
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.CannotIntroduceStaticVirtualMember.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, this.Builder),
                        this ) );
            }

            // Check that static member is not sealed.
            if ( this.Builder is { IsStatic: true, IsSealed: true } )
            {
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.CannotIntroduceStaticSealedMember.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, this.Builder),
                        this ) );
            }

            // Check that instance member is not introduced to a static type.
            if ( targetDeclaration.IsStatic && !this.Builder.IsStatic )
            {
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.CannotIntroduceInstanceMember.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration),
                        this ) );
            }

            // Check that virtual member is not introduced to a sealed type or a struct.
            if ( targetDeclaration is { IsSealed: true } or { TypeKind: TypeKind.Struct or TypeKind.RecordStruct }
                 && this.Builder.IsVirtual )
            {
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.CannotIntroduceVirtualToTargetType.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration),
                        this ) );
            }
        }

        public override string ToString() => $"Introduce {this.Builder}";
    }
}