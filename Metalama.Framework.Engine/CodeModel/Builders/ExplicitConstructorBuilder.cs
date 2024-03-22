// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class ExplicitConstructorBuilder : ConstructorBuilder
{
    private readonly IConstructorImpl _originalConstructor;

    public override Ref<IDeclaration> ToRef() => this._originalConstructor.ToRef();

    public ExplicitConstructorBuilder( INamedType targetType, Advice advice ) : base( targetType, advice )
    {
        Invariant.Assert( targetType.Constructors.All( c => c.IsImplicitlyDeclared ) );

        this._originalConstructor = (IConstructorImpl) targetType.Constructors.First( c => c.Parameters.Count == 0 );
    }
}