﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class EventBuilderData : MemberBuilderData
{
    private readonly IFullRef<IEvent> _ref;

    public ImmutableArray<IAttributeData> FieldAttributes { get; }

    public IRef<INamedType> Type { get; }

    public IObjectReader InitializerTags { get; }

    public bool IsEventField { get; }

    public RefKind RefKind { get; }

    public MethodBuilderData AddMethod { get; }

    public MethodBuilderData RemoveMethod { get; }

    public IRef<IEvent>? OverriddenEvent { get; }

    public IReadOnlyList<IRef<IEvent>> ExplicitInterfaceImplementations { get; }

    public IExpression? InitializerExpression { get; }

    public TemplateMember<IEvent>? InitializerTemplate { get; }

    public EventBuilderData( EventBuilder builder, IFullRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new BuiltDeclarationRef<IEvent>( this, containingDeclaration.CompilationContext );

        this.FieldAttributes = builder.FieldAttributes.ToImmutableArray();
        this.Type = builder.Type.ToRef();
        this.InitializerTags = builder.InitializerTags;
        this.RefKind = builder.RefKind;
        this.AddMethod = new MethodBuilderData( builder.AddMethod, this._ref );
        this.RemoveMethod = new MethodBuilderData( builder.RemoveMethod, this._ref );
        this.OverriddenEvent = builder.OverriddenEvent?.ToRef();
        this.ExplicitInterfaceImplementations = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );
        this.IsEventField = builder.IsEventField;
        this.InitializerExpression = builder.InitializerExpression;
        this.InitializerTemplate = builder.InitializerTemplate;
        this.Attributes = builder.Attributes.ToImmutable( this._ref );
    }

    protected override IFullRef<IDeclaration> ToDeclarationRef() => this._ref;

    public new IFullRef<IEvent> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Event;

    public override IRef<IMember>? OverriddenMember => this.OverriddenEvent;

    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementations;

    public override IEnumerable<DeclarationBuilderData> GetOwnedDeclarations() => base.GetOwnedDeclarations().Concat( [this.AddMethod, this.RemoveMethod] );
}