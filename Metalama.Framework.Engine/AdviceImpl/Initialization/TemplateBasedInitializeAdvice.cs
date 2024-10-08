// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Initialization;

internal sealed class TemplateBasedInitializeAdvice : InitializeAdvice
{
    private readonly BoundTemplateMethod _boundTemplate;
    private readonly IObjectReader _tags;

    public TemplateBasedInitializeAdvice(
        AdviceConstructorParameters<IMemberOrNamedType> parameters,
        BoundTemplateMethod boundTemplate,
        InitializerKind kind,
        IObjectReader tags )
        : base( parameters, kind )
    {
        this._boundTemplate = boundTemplate;
        this._tags = tags;
    }

    protected override void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation )
    {
        var initialization = new TemplateBasedInitializationTransformation(
            this.AdviceInfo,
            targetDeclaration.ToRef(),
            targetCtor.ToFullRef(),
            this._boundTemplate,
            this._tags );

        addTransformation( initialization );
    }
}