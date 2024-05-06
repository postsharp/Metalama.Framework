// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal sealed class OverrideIndexerAdvice : OverrideMemberAdvice<IIndexer, IIndexer>
{
    private readonly BoundTemplateMethod? _getTemplate;
    private readonly BoundTemplateMethod? _setTemplate;

    public OverrideIndexerAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        IIndexer targetDeclaration,
        ICompilation sourceCompilation,
        BoundTemplateMethod? getTemplate,
        BoundTemplateMethod? setTemplate,
        string? layerName,
        IObjectReader tags )
        : base( aspectInstance, templateInstance, targetDeclaration, sourceCompilation, layerName, tags )
    {
        this._getTemplate = getTemplate.ExplicitlyImplementedOrNull();
        this._setTemplate = setTemplate.ExplicitlyImplementedOrNull();
    }

    public override AdviceKind AdviceKind => AdviceKind.OverrideFieldOrPropertyOrIndexer;

    protected override OverrideMemberAdviceResult<IIndexer> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        addTransformation( new OverrideIndexerTransformation( this, targetDeclaration, this._getTemplate, this._setTemplate, this.Tags ) );

        return this.CreateSuccessResult( targetDeclaration );
    }
}