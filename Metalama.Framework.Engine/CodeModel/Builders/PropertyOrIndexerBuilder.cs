// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.ReflectionMocks;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class PropertyOrIndexerBuilder : MemberBuilder, IPropertyOrIndexerBuilder, IPropertyOrIndexerImpl
{
    private IType _type;

    public bool HasInitOnlySetter { get; protected set; }

    public RefKind RefKind { get; set; }

    public IMethodBuilder? GetMethod { get; }

    public IMethodBuilder? SetMethod { get; }

    IRef<IFieldOrPropertyOrIndexer> IFieldOrPropertyOrIndexer.ToRef() => this.ToFieldOrPropertyOrIndexerRef();

    protected abstract IRef<IFieldOrPropertyOrIndexer> ToFieldOrPropertyOrIndexerRef();

    public abstract Writeability Writeability { get; set; }

    public IType Type
    {
        get => this._type;
        set
        {
            this.CheckNotFrozen();

            this._type = this.Translate( value );
        }
    }

    IMethod? IFieldOrPropertyOrIndexer.GetMethod => this.GetMethod;

    IMethod? IFieldOrPropertyOrIndexer.SetMethod => this.SetMethod;

    protected PropertyOrIndexerBuilder(
        Advice advice,
        INamedType targetType,
        string name,
        bool hasGetter,
        bool hasSetter,
        bool hasImplicitGetter,
        bool hasImplicitSetter )
        : base( targetType, name, advice )
    {
        Invariant.Assert( hasGetter || hasSetter );

        this._type = targetType.Compilation.GetCompilationModel().Cache.SystemObjectType;

        if ( hasGetter )
        {
            this.GetMethod = new AccessorBuilder( this, MethodKind.PropertyGet, hasImplicitGetter );
        }

        if ( hasSetter )
        {
            this.SetMethod = new AccessorBuilder( this, MethodKind.PropertySet, hasImplicitSetter );
        }

        this.HasInitOnlySetter = false;
    }

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

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

    public abstract IRef<IPropertyOrIndexer> ToPropertyOrIndexerRef();

    public override void Freeze()
    {
        base.Freeze();

        ((DeclarationBuilder?) this.GetMethod)?.Freeze();
        ((DeclarationBuilder?) this.SetMethod)?.Freeze();
    }

    public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

    IRef<IPropertyOrIndexer> IPropertyOrIndexer.ToRef() => this.ToPropertyOrIndexerRef();
}