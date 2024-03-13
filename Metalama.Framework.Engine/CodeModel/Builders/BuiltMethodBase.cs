// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BuiltMethodBase : BuiltMember, IMethodBaseImpl
{
    public BuiltMethodBase( MethodBaseBuilder methodBaseBuilder, CompilationModel compilation ) : base( compilation, methodBaseBuilder )
    {
    }

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.MethodBaseBuilder;

    protected override MemberBuilder MemberBuilder => this.MethodBaseBuilder;

    protected abstract MethodBaseBuilder MethodBaseBuilder { get; }

    public abstract IParameterList Parameters { get; }

    public abstract System.Reflection.MethodBase ToMethodBase();
}