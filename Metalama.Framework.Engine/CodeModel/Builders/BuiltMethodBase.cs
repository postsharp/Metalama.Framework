// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BuiltMethodBase : BuiltMember, IMethodBaseImpl
{
    protected BuiltMethodBase( CompilationModel compilation ) : base( compilation ) { }

    protected abstract MethodBaseBuilder MethodBaseBuilder { get; }

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.GetCompilationModel().GetParameterCollection( this.MethodBaseBuilder.ToTypedRef<IHasParameters>() ) );

    public abstract System.Reflection.MethodBase ToMethodBase();
}