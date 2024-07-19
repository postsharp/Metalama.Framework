// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal sealed class OverrideFieldOrPropertyAdvice : OverrideMemberAdvice<IFieldOrProperty, IProperty>
{
    private readonly BoundTemplateMethod? _getTemplate;
    private readonly BoundTemplateMethod? _setTemplate;

    public OverrideFieldOrPropertyAdvice(
        AdviceConstructorParameters<IFieldOrProperty> parameters,
        BoundTemplateMethod? getTemplate,
        BoundTemplateMethod? setTemplate,
        IObjectReader tags )
        : base( parameters, tags )
    {
        this._getTemplate = getTemplate.ExplicitlyImplementedOrNull();
        this._setTemplate = setTemplate.ExplicitlyImplementedOrNull();
    }

    public override AdviceKind AdviceKind => AdviceKind.OverrideFieldOrPropertyOrIndexer;

    protected override OverrideMemberAdviceResult<IProperty> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        // TODO: Translate templates to this compilation.
        // TODO: order should be self if the target is introduced on the same layer.
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        var promotedField = OverrideHelper.OverrideProperty(
            serviceProvider,
            this,
            targetDeclaration.ForCompilation( compilation ).AssertNotNull(),
            this._getTemplate,
            this._setTemplate,
            this.Tags,
            addTransformation );

        return this.CreateSuccessResult( promotedField );
    }
}