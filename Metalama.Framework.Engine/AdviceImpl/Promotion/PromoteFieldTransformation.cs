// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class PromoteFieldTransformation : IntroducePropertyTransformation, IReplaceMemberTransformation
{
    private readonly IField _replacedField;

    public PromoteFieldTransformation( Advice advice, IField replacedField, PromotedField propertyBuilder ) : base( advice, propertyBuilder )
    {
        this._replacedField = replacedField;
    }

    public override InsertPosition InsertPosition => this._replacedField.ToInsertPosition();

    public MemberRef<IMember> ReplacedMember => this._replacedField.ToMemberRef<IMember>();

    public override IDeclaration TargetDeclaration => this._replacedField;
}