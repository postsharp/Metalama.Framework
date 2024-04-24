// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal class IntroduceNamedTypeAdvice : IntroduceMemberOrNamedTypeAdvice<INamedType, NamedTypeBuilder>
    {
        public override AdviceKind AdviceKind => AdviceKind.IntroduceType;

        public IntroduceNamedTypeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            INamespaceOrNamedType? targetNamespaceOrType,
            string? explicitName,
            ICompilation sourceCompilation,
            Action<NamedTypeBuilder>? buildAction,
            string? layerName ) : base( aspect, templateInstance, targetNamespaceOrType, sourceCompilation, buildAction, layerName )
        {
            this.Builder = new NamedTypeBuilder( this, targetNamespaceOrType.AssertNotNull(), explicitName.AssertNotNull() );
        }

        public override void Initialize( in ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( serviceProvider, diagnosticAdder );

            this.BuildAction?.Invoke( this.Builder );
        }

        public override AdviceImplementationResult Implement( ProjectServiceProvider serviceProvider, CompilationModel compilation, Action<ITransformation> addTransformation )
        {
            addTransformation( this.Builder.ToTransformation() );

            return AdviceImplementationResult.Success( AdviceOutcome.Default, this.Builder );
        }
    }
}