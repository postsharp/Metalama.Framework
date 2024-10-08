// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal abstract class IntroduceDeclarationTransformation<T> : BaseSyntaxTreeTransformation, IIntroduceDeclarationTransformation,
                                                                IInjectMemberTransformation
    where T : NamedDeclarationBuilderData
{
    public T BuilderData { get; }

    protected IntroduceDeclarationTransformation( AdviceInfo advice, T introducedDeclaration ) : base( advice, introducedDeclaration.PrimarySyntaxTree.AssertNotNull() )
    {
        this.BuilderData = introducedDeclaration.AssertNotNull();
    }

    public abstract IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context );

    public virtual InsertPosition InsertPosition => this.BuilderData.ToInsertPosition();

    DeclarationBuilderData IIntroduceDeclarationTransformation.DeclarationBuilderData => this.BuilderData;

    public override IRef<IDeclaration> TargetDeclaration => this.BuilderData.ContainingDeclaration.AssertNotNull();

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.IntroduceMember;

    protected override FormattableString ToDisplayString( CompilationModel compilation )
        => $"Introduce {this.BuilderData.DeclarationKind.ToDisplayString()} '{this.BuilderData.Name}' into '{this.BuilderData.ContainingDeclaration.GetTarget( compilation ).ToDisplayString()}'.";

    public override string ToString() => $"{{{this.GetType().Name} Builder={{{this.BuilderData}}}";
}