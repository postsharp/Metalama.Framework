// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal class IntroduceIndexerAdvice : IntroduceMemberAdvice<IIndexer, IndexerBuilder>
    {
        private readonly BoundTemplateMethod? _getTemplate;
        private readonly BoundTemplateMethod? _setTemplate;

        public IntroduceIndexerAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamedType targetDeclaration,
            ICompilation sourceCompilation,
            IReadOnlyList<(IType Type, string Name)> indices,
            BoundTemplateMethod? getTemplate,
            BoundTemplateMethod? setTemplate,
            IntroductionScope scope,
            OverrideStrategy overrideStrategy,
            Action<IIndexerBuilder>? buildAction,
            string? layerName,
            IObjectReader tags )
            : base(
                aspect,
                templateInstance,
                targetDeclaration,
                sourceCompilation,
                "this[]",
                null,
                scope,
                overrideStrategy,
                buildAction,
                layerName,
                tags )
        {
            this._getTemplate = getTemplate;
            this._setTemplate = setTemplate;

            var hasGet = getTemplate != null;
            var hasSet = setTemplate != null;

            this.Builder = new IndexerBuilder(
                this,
                targetDeclaration,
                hasGet,
                hasSet );

            foreach ( var pair in indices )
            {
                this.Builder.AddParameter( pair.Name, pair.Type );
            }
        }

        public override AdviceKind AdviceKind => AdviceKind.IntroduceIndexer;

        protected override void InitializeCore( ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.InitializeCore( serviceProvider, diagnosticAdder );

            this.Builder.Type = (this._getTemplate?.Template.Declaration.ReturnType).AssertNotNull();

            this.Builder.Accessibility =
                this._getTemplate != null
                    ? this._getTemplate.Template.Accessibility
                    : this._setTemplate.AssertNotNull().Template.Accessibility;

            if ( this._getTemplate != null )
            {
                CopyTemplateAttributes( this._getTemplate.Template.Declaration, this.Builder.GetMethod!, serviceProvider );
                CopyTemplateAttributes( this._getTemplate.Template.Declaration.ReturnParameter, this.Builder.GetMethod!.ReturnParameter, serviceProvider );
            }

            // TODO: There should be a selection of value parameter.
            if ( this._setTemplate != null && this._setTemplate.Template.Declaration.Parameters.Count > 0 )
            {
                CopyTemplateAttributes( this._setTemplate.Template.Declaration, this.Builder.SetMethod!, serviceProvider );

                CopyTemplateAttributes(
                    this._setTemplate.Template.Declaration.Parameters[0],
                    this.Builder.SetMethod!.Parameters.Last(),
                    serviceProvider );

                CopyTemplateAttributes( this._setTemplate.Template.Declaration.ReturnParameter, this.Builder.SetMethod.ReturnParameter, serviceProvider );
            }

            // TODO: For get accessor template, we are ignoring accessibility of set accessor template because it can be easily incompatible.
        }

        protected override void ValidateBuilder( INamedType targetDeclaration, IDiagnosticAdder diagnosticAdder )
        {
            base.ValidateBuilder( targetDeclaration, diagnosticAdder );

            if ( this.Builder.Parameters.Count <= 0 )
            {
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.CannotIntroduceIndexerWithoutParameters.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration) ) );
            }

            if ( this.Builder.IsStatic )
            {
                diagnosticAdder.Report(
                    AdviceDiagnosticDescriptors.CannotIntroduceStaticIndexer.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration) ) );
            }
        }

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            // Determine whether we need introduction transformation (something may exist in the original code or could have been introduced by previous steps).
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            var existingDeclaration = targetDeclaration.FindClosestVisibleIndexer( this.Builder );

            // TODO: Introduce attributes that are added not present on the existing member?
            if ( existingDeclaration == null )
            {
                // There is no existing declaration.

                // Introduce and override using the template.
                var overrideIndexerTransformation = new OverrideIndexerTransformation(
                    this,
                    this.Builder,
                    this._getTemplate,
                    this._setTemplate,
                    this.Tags );

                addTransformation( this.Builder.ToTransformation() );
                addTransformation( overrideIndexerTransformation );

                return AdviceImplementationResult.Success( this.Builder );
            }
            else
            {
                if ( existingDeclaration is not { } existingIndexer )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceWithDifferentKind.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration, existingDeclaration.DeclarationKind) ) );
                }

                if ( !compilation.Comparers.Default.Equals( this.Builder.Type, existingIndexer.Type ) )
                {
                    return
                        AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.CannotIntroduceDifferentExistingReturnType.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                 existingDeclaration.DeclaringType, existingIndexer.Type) ) );
                }

                switch ( this.OverrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        // Produce fail diagnostic.
                        return
                            AdviceImplementationResult.Failed(
                                AdviceDiagnosticDescriptors.CannotIntroduceMemberAlreadyExists.CreateRoslynDiagnostic(
                                    targetDeclaration.GetDiagnosticLocation(),
                                    (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                     existingDeclaration.DeclaringType) ) );

                    case OverrideStrategy.Ignore:
                        // Do nothing.
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.New:
                        // If the existing declaration is in the current type, we fail, otherwise, declare a new method and override.
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overrideIndexerTransformation = new OverrideIndexerTransformation(
                                this,
                                existingIndexer,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            addTransformation( overrideIndexerTransformation );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
                        }
                        else
                        {
                            this.Builder.IsNew = true;
                            this.Builder.OverriddenIndexer = existingIndexer;

                            var overrideIndexerTransformation = new OverrideIndexerTransformation(
                                this,
                                this.Builder,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            addTransformation( this.Builder.ToTransformation() );
                            addTransformation( overrideIndexerTransformation );

                            return AdviceImplementationResult.Success( AdviceOutcome.New, this.Builder );
                        }

                    case OverrideStrategy.Override:
                        if ( ((IEqualityComparer<IType>) compilation.Comparers.Default).Equals( targetDeclaration, existingDeclaration.DeclaringType ) )
                        {
                            var overrideIndexerTransformation = new OverrideIndexerTransformation(
                                this,
                                existingIndexer,
                                this._getTemplate,
                                this._setTemplate,
                                this.Tags );

                            addTransformation( overrideIndexerTransformation );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
                        }
                        else if ( existingDeclaration.IsSealed || !existingDeclaration.IsOverridable() )
                        {
                            return
                                AdviceImplementationResult.Failed(
                                    AdviceDiagnosticDescriptors.CannotIntroduceOverrideOfSealed.CreateRoslynDiagnostic(
                                        targetDeclaration.GetDiagnosticLocation(),
                                        (this.Aspect.AspectClass.ShortName, this.Builder, targetDeclaration,
                                         existingDeclaration.DeclaringType) ) );
                        }
                        else
                        {
                            this.Builder.IsOverride = true;
                            this.Builder.IsNew = false;
                            this.Builder.OverriddenIndexer = existingIndexer;
                            var overriddenIndexer = new OverrideIndexerTransformation( this, this.Builder, this._getTemplate, this._setTemplate, this.Tags );

                            addTransformation( this.Builder.ToTransformation() );
                            addTransformation( overriddenIndexer );

                            return AdviceImplementationResult.Success( AdviceOutcome.Override );
                        }

                    default:
                        throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
                }
            }
        }
    }
}