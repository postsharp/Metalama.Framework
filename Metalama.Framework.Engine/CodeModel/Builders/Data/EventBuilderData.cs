using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class EventBuilderData : MemberBuilderData
{
    public ImmutableArray<IAttributeData> FieldAttributes { get; }

    public IRef<INamedType> Type { get; }

    public IObjectReader InitializerTags { get; }

    public bool IsEventField { get; }

    public RefKind RefKind
    {
        get;
    }

    public MethodBuilderData AddMethod { get; }

    public MethodBuilderData RemoveMethod { get; }

    public IRef<IEvent>? OverriddenEvent { get; }
    
    public IReadOnlyList<IRef<IEvent>> ExplicitInterfaceImplementations { get; }

    public EventBuilderData( EventBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        var me = this.ToDeclarationRef();

        this.FieldAttributes = builder.FieldAttributes.ToImmutableArray();
        this.Type = builder.Type.ToRef();
        this.InitializerTags = builder.InitializerTags;
        this.RefKind = builder.RefKind;
        this.AddMethod = new MethodBuilderData( builder.AddMethod, me );
        this.RemoveMethod = new MethodBuilderData( builder.RemoveMethod, me );
        this.OverriddenEvent = builder.OverriddenEvent?.ToRef();
        this.ExplicitInterfaceImplementations = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );
    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Event;

    public override IRef<IMember>? OverriddenMember => this.OverriddenEvent;
    
    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementations;
}