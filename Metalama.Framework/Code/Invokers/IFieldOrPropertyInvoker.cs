// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.Invokers
{
    /// <summary>
    /// Allows accessing the the value of fields or properties.
    /// </summary>
#pragma warning disable CS0612
    public interface IFieldOrPropertyInvoker : IInvoker, IExpression { }
#pragma warning restore CS0612
}