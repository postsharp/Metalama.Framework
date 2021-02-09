using System;
using System.Diagnostics.CodeAnalysis;
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
        [field: ThreadStatic]
        internal static object? ProceedImpl { get; set; }

        [field: ThreadStatic]
        internal static object? ExpansionContext { get; set; }

        private static InvalidOperationException NewInvalidOperationException() =>
            new InvalidOperationException( "Code calling this method has to be compiled using Caravela." );

        /// <summary>
        /// Gets information about the element of code to which the template has been applied.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        [field: ThreadStatic]
        [AllowNull]
        [TemplateKeyword]
        public static ITemplateContext target { get; internal set; }

        /// <summary>
        /// Injects the logic that has been intercepted. For instance, in an <see cref="OverrideMethodAspect"/>,
        /// calling <see cref="proceed"/> invokes the method being overridden. Note that the way how the
        /// logic is invoked (as a method call or inlining) is considered an implementation detail.
        /// </summary>
        /// <returns></returns>
        [Proceed]
        public static dynamic proceed() => ProceedImpl ?? throw NewInvalidOperationException();

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
    }
}
