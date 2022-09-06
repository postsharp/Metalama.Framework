// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Advised
{
    /// <summary>
    /// Represents the method being overwritten or introduced. This interface extends <see cref="IMethod"/> but introduces
    /// the <see cref="Invoke"/> method, which allows you to invoke the method.
    /// It also overrides the <see cref="Parameters"/> property to expose their <see cref="IExpression.Value"/> property.
    /// </summary>
    public interface IAdvisedMethod : IMethod
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        dynamic? Invoke( params dynamic?[] args );

        /// <summary>
        /// Gets the list of method parameters.
        /// </summary>
        new IAdvisedParameterList Parameters { get; }
    }
}