// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
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

    public IDeclaration ContainingDeclaration { get; set; }

    public bool IsDesignTime => false;
}