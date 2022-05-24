// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel;

internal abstract class PropertyOrIndexer : Member, IPropertyOrIndexer
{
    protected IPropertySymbol PropertySymbol { get; }

    public PropertyOrIndexer( IPropertySymbol symbol, CompilationModel compilation ) : base( compilation )
    {
        this.PropertySymbol = symbol;
    }

    public override ISymbol Symbol => this.PropertySymbol;

    public RefKind RefKind => this.PropertySymbol.RefKind.ToOurRefKind();

    public override bool IsImplicit => false;

    public override bool IsExplicitInterfaceImplementation => !this.PropertySymbol.ExplicitInterfaceImplementations.IsEmpty;

    [Memo]
    public IType Type => this.Compilation.Factory.GetIType( this.PropertySymbol.Type );

    [Memo]
    public IMethod? GetMethod => this.PropertySymbol.GetMethod == null ? null : this.Compilation.Factory.GetMethod( this.PropertySymbol.GetMethod );

    [Memo]
    public virtual IMethod? SetMethod
        => this.PropertySymbol switch
        {
            { IsReadOnly: true } when this.PropertySymbol.IsAutoProperty() => new PseudoSetter( (IPropertyImpl) this, Accessibility.Private ),
            { IsReadOnly: true } => null,
            _ => this.Compilation.Factory.GetMethod( this.PropertySymbol.SetMethod! )
        };

    public override MemberInfo ToMemberInfo() => this.ToPropertyInfo();

    public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

    public override string ToString() => this.PropertySymbol.ToString();

    [Memo]
    public Writeability Writeability
        => this.PropertySymbol switch
        {
            { IsReadOnly: true } when this.PropertySymbol.IsAutoProperty() => Writeability.ConstructorOnly,
            { IsReadOnly: true } => Writeability.None,
            { SetMethod: { IsInitOnly: true } _ } => Writeability.InitOnly,
            _ => Writeability.All
        };

    public override bool IsAsync => false;

    public IMethod? GetAccessor( MethodKind methodKind )
        => methodKind switch
        {
            MethodKind.PropertyGet => this.GetMethod,
            MethodKind.PropertySet => this.SetMethod,
            _ => null
        };

    public IEnumerable<IMethod> Accessors
    {
        get
        {
            if ( this.GetMethod != null )
            {
                yield return this.GetMethod;
            }

            if ( this.SetMethod != null )
            {
                yield return this.SetMethod;
            }
        }
    }
}