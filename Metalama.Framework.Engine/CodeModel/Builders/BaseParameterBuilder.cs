// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal abstract class BaseParameterBuilder : DeclarationBuilder, IParameterBuilder, IParameterImpl
{
    protected BaseParameterBuilder( Advice parentAdvice ) : base( parentAdvice ) { }

    public abstract string Name { get; set; }

    public abstract IType Type { get; set; }

    public abstract RefKind RefKind { get; set; }

    public abstract int Index { get; }

    public abstract TypedConstant DefaultValue { get; set; }

    public abstract bool IsParams { get; set; }

    public abstract IHasParameters DeclaringMember { get; }

    public abstract ParameterInfo ToParameterInfo();

    public abstract bool IsReturnParameter { get; }
}