// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Represents an <see cref="AspectInstance"/> that is materialized by a custom attribute.
    /// </summary>
    internal class AttributeAspectInstance : AspectInstance
    {
        public AttributeAspectInstance(
            IAspect aspect,
            in Ref<IDeclaration> target,
            AspectClass aspectClass,
            IAttribute attribute ) :
            base( aspect, target, aspectClass, new AspectPredecessor( AspectPredecessorKind.Attribute, attribute ) )
        {
        }

    }
}