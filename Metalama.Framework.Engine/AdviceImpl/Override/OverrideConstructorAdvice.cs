// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal sealed class OverrideConstructorAdvice : OverrideMemberAdvice<IConstructor, IConstructor>
{
    private readonly BoundTemplateMethod _boundTemplate;

    public OverrideConstructorAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        IConstructor targetDeclaration,
        ICompilation sourceCompilation,
        BoundTemplateMethod boundTemplate,
        string? layerName,
        IObjectReader tags ) : base( aspectInstance, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
    {
        this._boundTemplate = boundTemplate;
    }

    public override AdviceKind AdviceKind => AdviceKind.OverrideConstructor;

    protected override OverrideMemberAdviceResult<IConstructor> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var constructor = this.TargetDeclaration.GetTarget( compilation );

        if ( constructor.IsImplicitInstanceConstructor() )
        {
            // Missing implicit ctor.
            var builder = new ConstructorBuilder( this, constructor.DeclaringType ) { IsReplacingExisting = true };
            addTransformation( builder.ToTransformation() );
            constructor = builder;
        }

        addTransformation( new OverrideConstructorTransformation( this, constructor, this._boundTemplate, this.Tags ) );

        return this.CreateSuccessResult( constructor );
    }
}