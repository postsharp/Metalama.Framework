// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class RemoveAttributesAdvice : Advice<RemoveAttributesAdviceResult>
{
    private readonly INamedType _attributeType;

    public RemoveAttributesAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        INamedType attributeType,
        string? layerName ) : base(
        aspectInstance,
        template,
        targetDeclaration,
        sourceCompilation,
        layerName )
    {
        this._attributeType = attributeType;
    }

    public override AdviceKind AdviceKind => AdviceKind.RemoveAttributes;

    protected override RemoveAttributesAdviceResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        if ( targetDeclaration.Attributes.OfAttributeType( this._attributeType ).Any() )
        {
            addTransformation(
                new RemoveAttributesTransformation(
                    this,
                    targetDeclaration,
                    this._attributeType ) );
        }

        return new RemoveAttributesAdviceResult();
    }
}