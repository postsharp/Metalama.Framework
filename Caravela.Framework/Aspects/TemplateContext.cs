// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable InconsistentNaming

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes the meta-model and the meta-functions to a template method.
    /// It is recommended to import this type using <c>using static</c>.
    /// </summary>
    [CompileTimeOnly]
    public static class TemplateContext
    {
        private static readonly AsyncLocal<ITemplateContextTarget?> _target = new();

        private static readonly AsyncLocal<object?> _proceedImplementation = new();

        // TODO: update the exception message.
        private static InvalidOperationException NewInvalidOperationException() => new( "Code accessing this member has to be compiled using Caravela." );

        /// <summary>
        /// Gets information about the element of code to which the template has been applied.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        [TemplateKeyword]
        public static ITemplateContextTarget target => _target.Value ?? throw NewInvalidOperationException();

        /// <summary>
        /// Injects the logic that has been intercepted. For instance, in an <see cref="OverrideMethodAspect"/>,
        /// calling <see cref="proceed"/> invokes the method being overridden. Note that the way how the
        /// logic is invoked (as a method call or inlining) is considered an implementation detail.
        /// </summary>
        /// <returns></returns>
        [Proceed]
        public static dynamic proceed() => _proceedImplementation.Value ?? throw NewInvalidOperationException();

        /// <summary>
        /// Coerces an <paramref name="expression"/> to be interpreted at compile time. This is typically used
        /// to coerce expressions that can be either run-time or compile-time. Since ambiguous expressions are
        /// interpreted as run-time by default, this method allows to change that behavior.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Exactly <paramref name="expression"/>, but coerced as a compile-time expression.</returns>
        [TemplateKeyword]
        [return: NotNullIfNotNull( "expression" )]
        public static T? compileTime<T>( T? expression ) => expression;

        [TemplateKeyword]
        public static T? runTime<T>( T? expression ) => expression;

        // Calls to pragma are purely syntactic, they are never executed. They are interpreted by the template compiler.
        [TemplateKeyword]
        public static ITemplateContextPragma pragma => null!;

#pragma warning restore IDE1006 // Naming Styles

        internal static void Initialize( ITemplateContextTarget targetImpl, object proceedImpl )
        {
            _target.Value = targetImpl;
            _proceedImplementation.Value = proceedImpl;
        }

        internal static void Close()
        {
            _target.Value = null;
            _proceedImplementation.Value = null;
        }
    }
}