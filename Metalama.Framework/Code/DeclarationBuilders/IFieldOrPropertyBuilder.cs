// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.DeclarationBuilders
{
    /// <summary>
    /// Base interface for <see cref="IFieldBuilder"/> and <see cref="IPropertyBuilder"/>.
    /// </summary>
    public interface IFieldOrPropertyBuilder : IFieldOrProperty, IFieldOrPropertyOrIndexerBuilder
    {
        /// <summary>
        /// Gets or sets the initializer expression (i.e. the expression at the right hand of the equal sign).
        /// </summary>
        new IExpression? InitializerExpression { get; set; }

        new bool IsRequired { get; set; }
    }
}