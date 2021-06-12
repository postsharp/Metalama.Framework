// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Advised
{
    /// <summary>
    /// Represents the field or property being overwritten or introduced. This interface introduces
    /// the <see cref="IHasRuntimeValue.Value"/> property, which allows you to read or write the field or property.
    /// </summary>
    public interface IAdviceFieldOrProperty : IFieldOrProperty, IHasRuntimeValue { }
}