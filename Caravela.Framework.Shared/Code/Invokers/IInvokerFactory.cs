// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Invokers
{
    /// <summary>
    /// Gives access to invokers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInvokerFactory<out T>
        where T : IInvoker
    {
        /// <summary>
        /// Gets the invoker for the base implementation of the declaration, i.e. <i>before</i> the application
        /// of the current aspect layer. To access the current layer, use <see cref="Final"/>. This property uses the unconditional
        /// access operator <c>.</c>. For null-conditional access, use <see cref="BaseConditional"/>.
        /// </summary>
        T? Base { get; }

        /// <summary>
        /// Gets the invoker for the base implementation of the declaration, i.e. <i>before</i> the application
        /// of the current aspect layer. To access the current layer, use <see cref="Final"/>. This property uses the null-conditional
        /// access operator <c>?.</c>. For unconditional access, use <see cref="Base"/>.
        /// </summary>
        T? BaseConditional { get; }

        /// <summary>
        /// Gets the invoker for the final implementation of the declaration, i.e. <i>after</i> the application
        /// of all aspects. If the member is <c>virtual</c>, the returned invoker performs a virtual call, therefore it calls the implementation on the child type
        /// (possibly with all applied aspects) is performed.  To access the prior layer (or the base type, if there is no prior layer), use <see cref="Base"/>.
        /// This property uses the unconditional access operator <c>.</c>. For null-conditional access, use <see cref="FinalConditional"/>.
        /// </summary>
        T Final { get; }

        /// <summary>
        /// Gets the invoker for the final implementation of the declaration, i.e. <i>after</i> the application
        /// of all aspects. If the member is <c>virtual</c>, the returned invoker performs a virtual call, therefore it calls the implementation on the child type
        /// (possibly with all applied aspects) is performed.  To access the prior layer (or the base type, if there is no prior layer), use <see cref="Base"/>.
        /// This property uses the null-conditional access operator <c>?.</c>. For unconditional access, use <see cref="Final"/>.
        /// </summary>
        T FinalConditional { get; }

        /// <summary>
        /// Gets the invoker for a given <see cref="InvokerOrder"/> and <see cref="InvokerOperator"/>.
        /// </summary>
        T? GetInvoker( InvokerOrder order, InvokerOperator @operator );
    }
}