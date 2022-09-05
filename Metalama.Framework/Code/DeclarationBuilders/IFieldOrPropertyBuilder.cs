// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Base interface for <see cref="IFieldBuilder"/> and <see cref="IPropertyBuilder"/>.
    /// </summary>
    public interface IFieldOrPropertyBuilder : IFieldOrProperty, IMemberBuilder
    {
        /// <summary>
        /// Gets or sets the type of the field or property.
        /// </summary>
        new IType Type { get; set; }

        IExpression? InitializerExpression { get; set; }
    }
}