// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Advices;

internal class RemoveAttributesAdvice : Advice
{
    private readonly INamedType _attributeType;

    public RemoveAttributesAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        INamedType attributeType,
        string? layerName ) : base(
        aspect,
        template,
        targetDeclaration,
        layerName,
        ObjectReader.Empty )
    {
        this._attributeType = attributeType;
    }

    public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

    public override AdviceResult ToResult( IServiceProvider serviceProvider, ICompilation compilation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        if ( targetDeclaration.Attributes.OfAttributeType( this._attributeType ).Any() )
        {
            return AdviceResult.Create(
                new RemoveAttributesTransformation(
                    this,
                    targetDeclaration,
                    this._attributeType,
                    targetDeclaration.GetDeclaringSyntaxReferences().Select( x => x.SyntaxTree ).Distinct().ToImmutableArray() ) );
        }
        else
        {
            return AdviceResult.Empty;
        }
    }
}