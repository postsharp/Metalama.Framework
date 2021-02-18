using System;
using Caravela.Framework.Project;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes the meta-model and the meta-functions to a template method.
    /// It is recommended to import this type using <c>using static</c>.
    /// </summary>
    [CompileTime]
    public static class TemplateContext
    {
        [ThreadStatic]
        private static ITemplateContext? _target;

        [ThreadStatic]
        private static object? _proceedImplementation;

        // TODO: update the exception message.
        private static InvalidOperationException NewInvalidOperationException() =>
            new InvalidOperationException( "Code accessing this member has to be compiled using Caravela." );

        /// <summary>
        /// Gets information about the element of code to which the template has been applied.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        [TemplateKeyword]
        public static ITemplateContext target => _target ?? throw NewInvalidOperationException();

        /// <summary>
        /// Injects the logic that has been intercepted. For instance, in an <see cref="OverrideMethodAspect"/>,
        /// calling <see cref="proceed"/> invokes the method being overridden. Note that the way how the
        /// logic is invoked (as a method call or inlining) is considered an implementation detail.
        /// </summary>
        /// <returns></returns>
        [Proceed]
        public static dynamic proceed() => _proceedImplementation ?? throw NewInvalidOperationException();

        /// <summary>
        /// Coerces an <paramref name="expression"/> to be interpreted at compile time. This is typically used
        /// to coerce expressions that can be either run-time or compile-time. Since ambiguous expressions are
        /// interpreted as run-time by default, this method allows to change that behavior.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Exactly <paramref name="expression"/>, but coerced as a compile-time expression.</returns>
        [TemplateKeyword]
        public static T compileTime<T>( T expression ) => expression;

#pragma warning restore IDE1006 // Naming Styles

        internal static void Initialize( ITemplateContext templateContext, object proceedImplementation )
        {
            _target = templateContext;
            _proceedImplementation = proceedImplementation;
        }

        internal static void Close()
        {
            _target = null;
            _proceedImplementation = null;
        }
    }
}
