// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class MethodBaseBuilder : MemberBuilder, IMethodBaseBuilder, IMethodBaseImpl
{
    public ParameterBuilderList Parameters { get; } = new();

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

    public abstract System.Reflection.MethodBase ToMethodBase();

    protected MethodBaseBuilder(
        Advice advice,
        INamedType targetType,
        string name )
        : base( targetType, name, advice ) { }

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
    {
        var parameterTypes = this.Parameters.AsEnumerable<IParameter>().Select( p => p.Type );

        return DisplayStringFormatter.Format( format, context, $"{this.DeclaringType}.{this.Name}({parameterTypes})" );
    }
}