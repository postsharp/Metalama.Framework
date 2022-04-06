// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the method being overwritten or introduced. This interface extends <see cref="IMethod"/> but introduces
    /// the <see cref="Invoke"/> method, which allows you to invoke the method.
    /// It also overrides the <see cref="Parameters"/> property to expose their <see cref="IExpression.Value"/> property.
    /// </summary>
    public interface IAdvisedConstructor : IConstructor
    {
    }
}