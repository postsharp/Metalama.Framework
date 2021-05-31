// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable UnusedParameter.Global

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes the meta-model and the meta-functions to a template method.
    /// </summary>
    [CompileTimeOnly]
    [TemplateKeyword]
#pragma warning disable SA1300, IDE1006 // Element should begin with upper-case letter

    // ReSharper disable once InconsistentNaming
    public static class meta
#pragma warning restore SA1300, IDE1006 // Element should begin with upper-case letter
    {
        private static readonly AsyncLocal<IMetaApi?> _currentContext = new();

        private static readonly AsyncLocal<object?> _proceedImplementation = new();

        private static IMetaApi CurrentContext => _currentContext.Value ?? throw NewInvalidOperationException();

        private static InvalidOperationException NewInvalidOperationException() => new( "The 'meta' API can be used only in the execution context of a template." );

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

        public static IFieldOrProperty FieldOrProperty => CurrentContext.FieldOrProperty;

        public static IMemberOrNamedType Member => CurrentContext.Member;

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
        /// Gets a <c>dynamic</c> object that represents an instance of the target type. It can be used as a value (e.g. as a method argument)
        /// or can be used to get access to <i>instance</i> members of the instance (e.g. <c>meta.This.MyMethod()</c>).
        /// The <see cref="This"/> property exposes the state of the target type as it is <i>after</i> the application
        /// of the current aspect layer. To access the prior layer (or the base type, if there is no prior layer), use <see cref="Base"/>.
        /// To access static members, use <see cref="ThisStatic"/>.
        /// </summary>
        /// <seealso cref="Base"/>
        /// <seealso cref="ThisStatic"/>
        [RunTimeOnly]
        public static dynamic This => CurrentContext.This;

        /// <summary>
        /// Gets a <c>dynamic</c> object that must be used to get access to <i>instance</i> members of the instance (e.g. <c>meta.Base.MyMethod()</c>).
        /// The <see cref="Base"/> property exposes the state of the target type as it is <i>before</i> the application
        /// of the current aspect layer. To access the current layer, use <see cref="This"/>.
        /// To access static members, use <see cref="BaseStatic"/>.
        /// </summary>
        /// <seealso cref="This"/>
        /// <seealso cref="BaseStatic"/>
        [RunTimeOnly]
        public static dynamic Base => CurrentContext.Base;

        /// <summary>
        /// Gets a <c>dynamic</c> object that must be used to get access to <i>static</i> members of the type (e.g. <c>meta.ThisStatic.MyStaticMethod()</c>).
        /// The <see cref="ThisStatic"/> property exposes the state of the target type as it is <i>after</i> the application
        /// of the current aspect layer. To access the prior layer (or the base type, if there is no prior layer), use <see cref="BaseStatic"/>.
        /// To access instance members, use <see cref="This"/>.
        /// </summary>
        /// <seealso cref="This"/>
        /// <seealso cref="BaseStatic"/>
        [RunTimeOnly]
        public static dynamic ThisStatic => CurrentContext.ThisStatic;

        /// <summary>
        /// Gets a <c>dynamic</c> object that must be used to get access to <i>static</i> members of the type (e.g. <c>meta.BaseStatic.MyStaticMethod()</c>).
        /// The <see cref="BaseStatic"/> property exposes the state of the target type as it is <i>before</i> the application
        /// of the current aspect layer. To access the current layer, use <see cref="ThisStatic"/>.
        /// To access instance members, use <see cref="Base"/>.
        /// </summary>
        /// <seealso cref="Base"/>
        /// <seealso cref="ThisStatic"/>
        [RunTimeOnly]
        public static dynamic BaseStatic => CurrentContext.BaseStatic;

        /// <summary>
        /// Gets a service allowing to report and suppress diagnostics.
        /// </summary>
        public static IDiagnosticSink Diagnostics => CurrentContext.Diagnostics;

        /// <summary>
        /// Gets the dictionary of tags that were passed by the <see cref="IAspect{T}.BuildAspect"/> method using 
        /// <see cref="AdviceOptions.Tags"/>.
        /// </summary>
        public static IReadOnlyDictionary<string, object?> Tags => CurrentContext.Tags;

        /// <summary>
        /// Gets the list of aspect aspects that have required the current aspect.
        /// </summary>
        [Obsolete( "Not implemented." )]
        public static IReadOnlyList<IAspectInstance> UpstreamAspects => throw new NotImplementedException();

        /// <summary>
        /// Injects a comment to the target code.
        /// </summary>
        /// <param name="lines">A list of comment lines, without the <c>//</c> prefix. Null strings are processed as blank ones and will inject a blank comment line.</param>
        /// <remarks>
        /// This method is not able to add a comment to an empty block. The block must contain at least one statement.
        /// </remarks>
        [TemplateKeyword]
        public static void Comment( params string?[] lines ) => throw NewInvalidOperationException();

        internal static IDisposable WithContext( IMetaApi current, object proceedImpl )
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