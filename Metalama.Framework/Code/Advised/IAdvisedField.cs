// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the field being overwritten or introduced. This interface extends <see cref="IField"/> but introduces
    /// the <see cref="IExpression.Value"/> property, which allows you to read or write the field.
    /// </summary>
    public interface IAdvisedField : IField, IAdvisedFieldOrProperty { }
}