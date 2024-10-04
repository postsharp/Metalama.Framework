// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltEvent : BuiltMember, IEventImpl
{
    public EventBuilderData EventBuilder { get; }

    public BuiltEvent( EventBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.EventBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this.EventBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this.EventBuilder;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this.EventBuilder;

    protected override MemberBuilderData MemberBuilder => this.EventBuilder;

    [Memo]
    public INamedType Type => this.MapDeclaration( this.EventBuilder.Type );

    public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

    [Memo]
    public IMethod AddMethod => new BuiltAccessor( this, this.EventBuilder.AddMethod );

    [Memo]
    public IMethod RemoveMethod => new BuiltAccessor( this, this.EventBuilder.RemoveMethod );

    public IMethod? RaiseMethod => null;

    [Memo]
    public IEvent? OverriddenEvent => this.MapDeclaration( this.EventBuilder.OverriddenEvent );

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IEvent> ExplicitInterfaceImplementations
        => this.EventBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( this.MapDeclaration );

    [Memo]
    public IEvent Definition => this.Compilation.Factory.GetEvent( this.EventBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    public EventInfo ToEventInfo() => this.EventBuilder.ToEventInfo();

    [Memo]
    private IRef<IEvent> Ref => this.RefFactory.FromBuilt<IEvent>( this );

    public IRef<IEvent> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public IEventInvoker With( InvokerOptions options ) => this.EventBuilder.With( options );

    public IEventInvoker With( object? target, InvokerOptions options = default ) => this.EventBuilder.With( target, options );

    public object Add( object? handler ) => this.EventBuilder.Add( handler );

    public object Remove( object? handler ) => this.EventBuilder.Remove( handler );

    public object Raise( params object?[] args ) => this.EventBuilder.Raise( args );

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

    IType IHasType.Type => this.Type;

    RefKind IHasType.RefKind => RefKind.None;
}