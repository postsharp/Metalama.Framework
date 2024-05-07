// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class EventBuilder : MemberBuilder, IEventBuilder, IEventImpl
{
    private readonly List<IAttributeData> _fieldAttributes;
    private INamedType _type;

    public IObjectReader InitializerTags { get; }

    public bool IsEventField { get; }

    public IReadOnlyList<IAttributeData> FieldAttributes => this._fieldAttributes;

    public EventBuilder(
        Advice advice,
        INamedType targetType,
        string name,
        bool isEventField,
        IObjectReader initializerTags )
        : base( targetType, name, advice )
    {
        this.InitializerTags = initializerTags;
        this.IsEventField = isEventField;
        this._type = (INamedType) targetType.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(EventHandler) );
        this._fieldAttributes = new List<IAttributeData>();
    }

    public void AddFieldAttribute( IAttributeData attributeData ) => this._fieldAttributes.Add( attributeData );

    public INamedType Type
    {
        get => this._type;
        set => this._type = this.Translate( value );
    }

    public RefKind RefKind
    {
        get => RefKind.None;
        set => throw new NotSupportedException();
    }

    public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

    [Memo]
    public IMethodBuilder AddMethod => new AccessorBuilder( this, MethodKind.EventAdd, this.IsEventField );

    [Memo]
    public IMethodBuilder RemoveMethod => new AccessorBuilder( this, MethodKind.EventRemove, this.IsEventField );

    public IMethodBuilder? RaiseMethod => null;

    public IEvent? OverriddenEvent { get; set; }

    public override DeclarationKind DeclarationKind => DeclarationKind.Event;

    IType IHasType.Type => this.Type;

    IType IHasTypeBuilder.Type
    {
        get => this.Type;

        set
        {
            switch ( value )
            {
                case INamedType namedType:
                    this.Type = this.Translate( namedType );

                    break;

                default:
                    throw new InvalidOperationException( "IEventBuilder.Type cannot be set to a value that does not implement INamedType." );
            }
        }
    }

    RefKind IHasType.RefKind => this.RefKind;

    IMethod IEvent.AddMethod => this.AddMethod;

    IMethod IEvent.RemoveMethod => this.RemoveMethod;

    IMethod? IEvent.RaiseMethod => this.RaiseMethod;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    public IReadOnlyList<IEvent> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IEvent>();

    IEvent IEvent.Definition => this;

    public IExpression? InitializerExpression { get; set; }

    public TemplateMember<IEvent>? InitializerTemplate { get; set; }

    public EventInfo ToEventInfo() => CompileTimeEventInfo.Create( this );

    public IEventInvoker With( InvokerOptions options ) => new EventInvoker( this, options );

    public IEventInvoker With( object? target, InvokerOptions options = default ) => new EventInvoker( this, options, target );

    public object Add( object? handler ) => new EventInvoker( this ).Add( handler );

    public object Remove( object? handler ) => new EventInvoker( this ).Remove( handler );

    public object Raise( params object?[] args ) => new EventInvoker( this ).Raise( args );

    public void SetExplicitInterfaceImplementation( IEvent interfaceEvent ) => this.ExplicitInterfaceImplementations = new[] { interfaceEvent };

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    public override IMember? OverriddenMember => (IMemberImpl?) this.OverriddenEvent;

    public IInjectMemberTransformation ToTransformation() => new IntroduceEventTransformation( this.ParentAdvice, this );

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors
    {
        get
        {
            yield return this.AddMethod;
            yield return this.RemoveMethod;

            if ( this.RaiseMethod != null )
            {
                yield return this.RaiseMethod;
            }
        }
    }

    public override void Freeze()
    {
        base.Freeze();

        ((DeclarationBuilder?) this.AddMethod)?.Freeze();
        ((DeclarationBuilder?) this.RemoveMethod)?.Freeze();
    }
}