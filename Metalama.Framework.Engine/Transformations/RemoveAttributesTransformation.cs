// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;

namespace Metalama.Framework.Engine.Transformations;

internal class RemoveAttributesTransformation : IObservableTransformation
{
    public INamedType AttributeType { get; }

    public Advice Advice { get; }

    public RemoveAttributesTransformation(
        Advice advice,
        IDeclaration targetDeclaration,
        INamedType attributeType )
    {
        this.AttributeType = attributeType;
        this.ContainingDeclaration = targetDeclaration;
        this.Advice = advice;
    }

    public IDeclaration ContainingDeclaration { get; set; }

    public bool IsDesignTime => false;
}