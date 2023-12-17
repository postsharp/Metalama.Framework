// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis.Options;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal abstract class IntroduceMemberOrNamedTypeAdvice<TMemberOrNamedType, TBuilder> : Advice
        where TMemberOrNamedType : class, IMemberOrNamedType
        where TBuilder : MemberOrNamedTypeBuilder
    {
        protected TBuilder Builder { get; init; }

        public Action<TBuilder>? BuildAction { get; }

        protected IntroduceMemberOrNamedTypeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IDeclaration? targetDeclaration,
            ICompilation sourceCompilation,
            Action<TBuilder>? buildAction,
            string? layerName ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName )
        {
            this.BuildAction = buildAction;

            // This is to make the nullability analyzer happy. Derived classes are supposed to set this member in the
            // constructor. Other designs are more cumbersome.
            this.Builder = null!;
        }

        public override void Initialize( ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
        {
            base.Initialize( serviceProvider, diagnosticAdder );
        }

        protected virtual void ValidateBuilder( INamedType targetDeclaration, IDiagnosticAdder diagnosticAdder )
        {
        }

        protected static void CopyTemplateAttributes( IDeclaration declaration, IDeclarationBuilder builder, ProjectServiceProvider serviceProvider )
        {
            var classificationService = serviceProvider.Global.GetRequiredService<AttributeClassificationService>();

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