// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class PromoteFieldTransformation : IntroducePropertyTransformation, IReplaceMemberTransformation
{
    private readonly IField _replacedField;

    public PromoteFieldTransformation( Advice advice, IField replacedField, PromotedFieldBuilder propertyBuilder ) : base( advice, propertyBuilder )
    {
        this._replacedField = replacedField;
    }

    public override InsertPosition InsertPosition => this._replacedField.ToInsertPosition();

    IMember? IReplaceMemberTransformation.ReplacedMember => this._replacedField;

    public override IDeclaration TargetDeclaration => this._replacedField;
}