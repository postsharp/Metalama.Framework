// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// Represents an <see cref="AspectInstance"/> that is materialized by a custom attribute.
    /// </summary>
    internal class AttributeAspectInstance : AspectInstance
    {
        private readonly IAttribute _attribute;
        private readonly CompileTimeProjectLoader _loader;

        public AttributeAspectInstance(
            IAspect aspect,
            in Ref<IDeclaration> target,
            AspectClass aspectClass,
            IAttribute attribute,
            CompileTimeProjectLoader loader ) :
            base( aspect, target, aspectClass, new AspectPredecessor( AspectPredecessorKind.Attribute, attribute ) )
        {
            this._attribute = attribute;
            this._loader = loader;
        }

        private AttributeAspectInstance(
            IAspect aspect,
            in Ref<IDeclaration> target,
            AspectClass aspectClass,
            IAttribute attribute,
            in AspectPredecessor aspectPredecessor,
            CompileTimeProjectLoader loader ) : base( aspect, target, aspectClass, aspectPredecessor )
        {
            this._attribute = attribute;
            this._loader = loader;
        }

        public override AttributeAspectInstance CreateDerivedInstance( IDeclaration target )
        {
            var attributeData = this._attribute.GetAttributeData();

            if ( !this._loader.AttributeDeserializer.TryCreateAttribute( attributeData, NullDiagnosticAdder.Instance, out var attributeInstance ) )
            {
                // This should not happen because we've already deserialized the same attribute once before.
                throw new AssertionFailedException();
            }

            return new AttributeAspectInstance(
                (IAspect) attributeInstance,
                target.ToTypedRef(),
                (AspectClass) this.AspectClass,
                this._attribute,
                new AspectPredecessor( AspectPredecessorKind.Inherited, this ),
                this._loader );
        }
    }
}