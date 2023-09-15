// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltProperty : BuiltMember, IPropertyImpl
{
    public BuiltProperty( PropertyBuilder builder, CompilationModel compilation ) : base( compilation, builder )
    {
        this.PropertyBuilder = builder;
    }

    public PropertyBuilder PropertyBuilder { get; }

    protected override MemberBuilder MemberBuilder => this.PropertyBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.PropertyBuilder;

    public RefKind RefKind => this.PropertyBuilder.RefKind;

    public Writeability Writeability => this.PropertyBuilder.Writeability;

    public bool? IsAutoPropertyOrField => this.PropertyBuilder.IsAutoPropertyOrField;

    [Memo]
    public IType Type => this.Compilation.Factory.GetIType( this.PropertyBuilder.Type );

    [Memo]
    public IMethod? GetMethod => this.PropertyBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.GetMethod ) : null;

    [Memo]
    public IMethod? SetMethod => this.PropertyBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.SetMethod ) : null;

    [Memo]
    public IProperty? OverriddenProperty => this.Compilation.Factory.GetDeclaration( this.PropertyBuilder.OverriddenProperty );

    IProperty IProperty.PropertyDefinition => this;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
        => this.PropertyBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.PropertyBuilder.ToFieldOrPropertyInfo();

    public bool IsRequired => this.PropertyBuilder.IsRequired;

    public IExpression? InitializerExpression => this.PropertyBuilder.InitializerExpression;

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => this.PropertyBuilder.With( options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => this.PropertyBuilder.With( target, options );

    public ref object? Value => ref this.PropertyBuilder.Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => this.PropertyBuilder.ToTypedExpressionSyntax( syntaxGenerationContext );

    public PropertyInfo ToPropertyInfo() => this.PropertyBuilder.ToPropertyInfo();

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors => this.PropertyBuilder.Accessors.Select( a => this.Compilation.Factory.GetDeclaration( a ) );

    bool IExpression.IsAssignable => this.Writeability != Writeability.None;
}