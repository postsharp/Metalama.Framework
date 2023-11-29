// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class ReplaceWithFieldTransformation : IntroduceFieldTransformation, IReplaceMemberTransformation
{
    private readonly IField _replacedMember;

    public ReplaceWithFieldTransformation( Advice advice, IField replacedMember, FieldBuilder fieldBuilder )
        : base( advice, fieldBuilder )
    {
        this._replacedMember = replacedMember;
    }

    public override InsertPosition InsertPosition => this._replacedMember.ToInsertPosition();

    public MemberRef<IMember> ReplacedMember => this._replacedMember.ToMemberRef<IMember>();

    public override IDeclaration TargetDeclaration => this._replacedMember;
}