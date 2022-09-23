﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Engine.Transformations;

internal class RemoveAttributesTransformation : BaseTransformation, IObservableTransformation
{
    public INamedType AttributeType { get; }

    public RemoveAttributesTransformation(
        Advice advice,
        IDeclaration targetDeclaration,
        INamedType attributeType ) : base( advice )
    {
        this.AttributeType = attributeType;
        this.ContainingDeclaration = targetDeclaration;
    }

    public IDeclaration ContainingDeclaration { get; }

    public bool IsDesignTime => false;

    public override IDeclaration TargetDeclaration => this.ContainingDeclaration;
}