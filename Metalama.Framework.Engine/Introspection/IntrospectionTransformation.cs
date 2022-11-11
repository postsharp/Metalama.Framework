// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionTransformation : IIntrospectionTransformation
{
    private readonly ITransformation _transformation;
    private readonly ICompilation _compilation;

    public IntrospectionTransformation( ITransformation transformation, ICompilation compilation )
    {
        this._transformation = transformation;
        this._compilation = compilation;
    }

    public TransformationKind TransformationKind => this._transformation.TransformationKind;

    [Memo]
    public IDeclaration TargetDeclaration => this._compilation.GetCompilationModel().Factory.GetDeclaration( this._transformation.TargetDeclaration );

    [Memo]
    public string Description => UserMessageFormatter.Format( this._transformation.ToDisplayString() );

    public override string ToString() => this.Description;
}