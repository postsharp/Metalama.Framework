// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class MethodBaseBuilder : MemberBuilder, IMethodBaseBuilder, IMethodBaseImpl
{
    public ParameterBuilderList Parameters { get; } = new();

    public abstract BaseParameterBuilder? ReturnParameter { get; set; }

    public override void Freeze()
    {
        base.Freeze();

        foreach ( var parameter in this.Parameters )
        {
            parameter.Freeze();
        }
    }

    public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
    {
        this.CheckNotFrozen();

        var parameter = new ParameterBuilder( this, this.Parameters.Count, name, type, refKind, this.ParentAdvice );
        parameter.DefaultValue = defaultValue;
        this.Parameters.Add( parameter );

        return parameter;
    }

    public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = null )
    {
        this.CheckNotFrozen();

        var iType = this.Compilation.Factory.GetTypeByReflectionType( type );
        TypedConstant? typedConstant = defaultValue != null ? TypedConstant.Create( defaultValue.Value.Value, iType ) : null;

        return this.AddParameter( name, iType, refKind, typedConstant );
    }

    IParameterList IHasParameters.Parameters => this.Parameters;

    IParameterBuilderList IHasParametersBuilder.Parameters => this.Parameters;

    public abstract IRef<IMethodBase> ToMethodBaseRef();

    public abstract MethodBase ToMethodBase();

    public new IRef<IMethodBase> ToRef() => this.ToMethodBaseRef();

    protected MethodBaseBuilder(
        Advice advice,
        INamedType targetType,
        string name )
        : base( targetType, name, advice ) { }
}