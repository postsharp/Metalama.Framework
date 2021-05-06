// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable UnusedParameter.Global

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes the meta-model and the meta-functions to a template method.
    /// It is recommended to import this type using <c>using static</c>.
    /// </summary>
    [CompileTimeOnly]
    [TemplateKeyword]
#pragma warning disable SA1300, IDE1006 // Element should begin with upper-case letter

    // ReSharper disable once InconsistentNaming
    public static class meta
#pragma warning restore SA1300, IDE1006 // Element should begin with upper-case letter
    {
        private static readonly AsyncLocal<ITemplateContext?> _currentContext = new();

        private static readonly AsyncLocal<object?> _proceedImplementation = new();

        private static ITemplateContext CurrentContext => _currentContext.Value ?? throw NewInvalidOperationException();

        // TODO: update the exception message.
        private static InvalidOperationException NewInvalidOperationException() => new( "Code accessing this member has to be compiled using Caravela." );

        /// <summary>
        /// Injects the logic that has been intercepted. For instance, in an <see cref="OverrideMethodAspect"/>,
        /// calling <see cref="Proceed"/> invokes the method being overridden. Note that the way how the
        /// logic is invoked (as a method call or inlining) is considered an implementation detail.
        /// </summary>
        /// <returns></returns>
        [Proceed]
        [TemplateKeyword]
        [return: RunTimeOnly]
        public static dynamic Proceed() => _proceedImplementation.Value ?? throw NewInvalidOperationException();

        /// <summary>
        /// Coerces an <paramref name="expression"/> to be interpreted at compile time. This is typically used
        /// to coerce expressions that can be either run-time or compile-time, such as a literal. Since ambiguous expressions are
        /// interpreted as run-time by default, this method allows to change that behavior.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Exactly <paramref name="expression"/>, but coerced as a compile-time expression.</returns>
        [return: NotNullIfNotNull( "expression" )]
        [TemplateKeyword]
        public static T? CompileTime<T>( T? expression ) => expression;

        /// <summary>
        /// Converts a compile-value into run-time value by serializing the compile-time value into a some syntax that will
        /// evaluate, at run time, to the same value as at compile time.
        /// </summary>
        /// <param name="value">A compile-time value.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>A value that is structurally equivalent to the compile-time <paramref name="value"/>.</returns>
        [TemplateKeyword]
        public static T? RunTime<T>( T? value ) => value;

        /// <summary>
        /// Gets the method metadata, or the accessor if this is a template for a field, property or event.
        /// </summary>
        /// <remarks>
        /// To invoke the method, use <c>Invoke</c>.
        /// e.g. <c>OverrideMethodContext.Method.Invoke(1, 2, 3);</c>.
        /// </remarks>
        public static IMethod Method => CurrentContext.Method;

        /// <summary>
        /// Gets the target field or property, or null if the advice does not target a field or a property.
        /// </summary>
        public static IProperty Property => CurrentContext.Property;

        /// <summary>
        /// Gets the target event, or null if the advice does not target an event.
        /// </summary>
        public static IEvent Event => CurrentContext.Event;

        /// <summary>
        /// Gets the list of parameters of <see cref="Method"/>.
        /// </summary>
        public static IAdviceParameterList Parameters => CurrentContext.Parameters;

        // Gets the project configuration.
        // IProject Project { get; }

        /// <summary>
        /// Gets the code model of current type including the introductions of the current aspect type.
        /// </summary>
        public static INamedType Type => CurrentContext.Type;

        /// <summary>
        /// Gets the code model of the whole compilation.
        /// </summary>
        public static ICompilation Compilation => CurrentContext.Compilation;

        /// <summary>
        /// Gets an object that gives access to the current type including members introduced by the current aspect.
        /// Both instance and static members are made accessible. For instance members,
        /// the <c>this</c> instance is assumed.
        /// </summary>
        [RunTimeOnly]
        public static dynamic This => CurrentContext.This;

        /// <summary>
        /// Gets a service allowing to report and suppress diagnostics.
        /// </summary>
        public static IDiagnosticSink Diagnostics => CurrentContext;

        /// <summary>
        /// Injects a comment to the target code.
        /// </summary>
        /// <param name="lines">A list of comment lines, without the <c>//</c> prefix. Null strings are processed as blank ones and will inject a blank comment line.</param>
        /// <remarks>
        /// This method is not able to add a comment to an empty block. The block must contain at least one statement.
        /// </remarks>
        [Pragma]
        public static void Comment( params string?[] lines ) => throw NewInvalidOperationException();

        internal static IDisposable WithContext( ITemplateContext current, object proceedImpl )
        {
            _currentContext.Value = current;
            _proceedImplementation.Value = proceedImpl;

            return new InitializeCookie();
        }

        private class InitializeCookie : IDisposable
        {
            public void Dispose()
            {
                _currentContext.Value = null;
                _proceedImplementation.Value = null;
            }
        }
    }
}