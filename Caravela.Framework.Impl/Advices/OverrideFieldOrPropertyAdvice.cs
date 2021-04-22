// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl
{
    internal class OverrideFieldOrPropertyAdvice : Advice, IOverrideFieldOrPropertyAdvice
    {
        public IMethod GetTemplateMethod { get; }

        public IMethod SetTemplateMethod { get; }

        public new IFieldOrProperty TargetDeclaration => (IFieldOrProperty) base.TargetDeclaration;

        public AspectLinkerOptions? LinkerOptions { get; }

        public OverrideFieldOrPropertyAdvice(
            AspectInstance aspect,
            IFieldOrProperty targetDeclaration,
            IMethod getTemplateMethod,
            IMethod setTemplateMethod,
            AspectLinkerOptions aspectLinkerOptions ) : base( aspect, targetDeclaration )
        {
            this.GetTemplateMethod = getTemplateMethod;
            this.SetTemplateMethod = setTemplateMethod;
            this.LinkerOptions = aspectLinkerOptions;
        }

        public override AdviceResult ToResult( ICompilation compilation )
        {
            // TODO: Translate templates to this compilation.
            if ( this.TargetDeclaration is IField field )
            {
                var promotedField = new PromotedField( this, field, this.LinkerOptions );

                return AdviceResult.Create(
                    promotedField,
                    new OverriddenProperty( this, promotedField, this.GetTemplateMethod, this.SetTemplateMethod, this.LinkerOptions ) );
            }
            else if ( this.TargetDeclaration is IProperty property )
            {
                return AdviceResult.Create( new OverriddenProperty( this, property, this.GetTemplateMethod, this.SetTemplateMethod, this.LinkerOptions ) );
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
    }
}