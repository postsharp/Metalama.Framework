﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal class RemoveAttributesAdvice : Advice
{
    private readonly INamedType _attributeType;

    public RemoveAttributesAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        INamedType attributeType,
        string? layerName ) : base(
        aspect,
        template,
        targetDeclaration,
        sourceCompilation,
        layerName )
    {
        this._attributeType = attributeType;
    }

    public override AdviceImplementationResult Implement(
        IServiceProvider serviceProvider,
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

        return AdviceImplementationResult.Success();
    }
}