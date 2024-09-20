// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltEvent : BuiltMember, IEventImpl
{
    public EventBuilder EventBuilder { get; }

    public BuiltEvent( CompilationModel compilation, EventBuilder builder, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.EventBuilder = builder;
    }

    public override DeclarationBuilder Builder => this.EventBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this.EventBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.EventBuilder;

    protected override MemberBuilder MemberBuilder => this.EventBuilder;

    [Memo]
    public INamedType Type => this.MapType( this.EventBuilder.Type );

    public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

    [Memo]
    public IMethod AddMethod => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.AddMethod );

    [Memo]
    public IMethod RemoveMethod => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.RemoveMethod );

    public IMethod? RaiseMethod => null;

    [Memo]
    public IEvent? OverriddenEvent => this.Compilation.Factory.Translate( this.EventBuilder.OverriddenEvent );

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IEvent> ExplicitInterfaceImplementations
        => this.EventBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray(
            i => this.Compilation.Factory.Translate( i, genericContext: this.GenericContext ) );

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