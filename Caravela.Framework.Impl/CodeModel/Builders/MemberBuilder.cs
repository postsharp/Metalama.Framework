// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Impl.Advices;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal abstract class MemberBuilder : MemberOrNamedTypeBuilder, IMemberBuilder
    {
        protected MemberBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice, declaringType, name ) { }

        public new INamedType DeclaringType => base.DeclaringType.AssertNotNull();

        public override string ToString() => this.DeclaringType + "." + this.Name;

        public abstract bool IsExplicitInterfaceImplementation { get; }

        public bool IsVirtual { get; set; }

        public bool IsAsync { get; set; }

        public bool IsOverride { get; set; }

        public override bool IsDesignTime => !this.IsOverride;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.DeclaringType.ToDisplayString( format, context ) + "." + this.Name;

        public void ApplyTemplateAttribute( TemplateAttribute templateAttribute )
        {
            if ( templateAttribute.Name != null )
            {
                this.Name = templateAttribute.Name;
            }

            if ( templateAttribute.GetIsSealed().HasValue )
            {
                this.IsSealed = templateAttribute.GetIsSealed()!.Value;
            }

            if ( templateAttribute.GetAccessibility().HasValue )
            {
                this.Accessibility = templateAttribute.GetAccessibility()!.Value;
            }

            if ( templateAttribute.GetIsVirtual().HasValue )
            {
                this.IsVirtual = templateAttribute.GetIsVirtual().HasValue;
            }
        }
    }
}