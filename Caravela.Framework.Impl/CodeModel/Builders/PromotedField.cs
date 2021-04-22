// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class PromotedField : PropertyBuilder, IReplaceMemberTransformation
    {
        public IField ReplacedField { get; }

        MemberLink<IMember> IReplaceMemberTransformation.ReplacedMember => this.ReplacedField.ToMemberLink<IMember>();

        public PromotedField( Advice parentAdvice, IField field, AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, field.DeclaringType, field.Name, linkerOptions )
        {
            this.ReplacedField = field;
        }
    }
}
