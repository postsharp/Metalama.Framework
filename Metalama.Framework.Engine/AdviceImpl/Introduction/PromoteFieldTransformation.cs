// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class PromoteFieldTransformation : IntroducePropertyTransformation, IReplaceMemberTransformation
{
    private readonly IRef<IField> _replacedField;

    public PromoteFieldTransformation( AdviceInfo advice, IField replacedField, PropertyBuilderData propertyBuilder ) : base( advice, propertyBuilder )
    {
        this._replacedField = replacedField.ToRef();
    }

    public override InsertPosition InsertPosition => this._replacedField.ToInsertPosition();

    IRef<IMember>? IReplaceMemberTransformation.ReplacedMember => this._replacedField;

    public override IRef<IDeclaration> TargetDeclaration => this._replacedField;
}