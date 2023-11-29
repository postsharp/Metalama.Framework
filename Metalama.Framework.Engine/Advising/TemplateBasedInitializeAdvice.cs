// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class TemplateBasedInitializeAdvice : InitializeAdvice
    {
        private readonly BoundTemplateMethod _boundTemplate;
        private readonly IObjectReader _tags;

        public TemplateBasedInitializeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMemberOrNamedType targetDeclaration,
            ICompilation sourceCompilation,
            BoundTemplateMethod boundTemplate,
            InitializerKind kind,
            string? layerName,
            IObjectReader tags ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, kind, layerName )
        {
            this._boundTemplate = boundTemplate;
            this._tags = tags;
        }

        public override AdviceImplementationResult Implement( ProjectServiceProvider serviceProvider, CompilationModel compilation, Action<ITransformation> addTransformation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            if ( targetDeclaration is IConstructor { IsPrimary: true } or INamedType { PrimaryConstructor: not null } )
            {
                return AdviceImplementationResult.Failed(
                    AdviceDiagnosticDescriptors.CannotAddTemplateInitializerToPrimaryConstructor.CreateRoslynDiagnostic(
                        targetDeclaration.GetDiagnosticLocation(),
                        (this.Aspect.AspectClass.ShortName, targetDeclaration.GetClosestNamedType()),
                        this ) );
            }

            return base.Implement( serviceProvider, compilation, addTransformation );
        }

        protected override void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation )
        {
            var initialization = new TemplateBasedInitializationTransformation(
                this,
                targetDeclaration,
                targetCtor,
                this._boundTemplate,
                this._tags );

            addTransformation( initialization );
        }
    }
}