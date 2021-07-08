// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

// ReSharper disable UnusedParameter.Global

namespace Caravela.Framework.Aspects
{
    // ReSharper disable once InconsistentNaming

    /// <summary>
    /// The entry point for the meta model, which can be used in templates to inspect the target code or access other
    /// features of the template language.
    /// </summary>
    /// <seealso href="@templates"/>
    [CompileTimeOnly]
    [TemplateKeyword]
#pragma warning disable SA1300, IDE1006 // Element should begin with upper-case letter
    public static class meta
#pragma warning restore SA1300, IDE1006 // Element should begin with upper-case letter
    {
        private static readonly AsyncLocal<IMetaApi?> _currentContext = new();

        private static IMetaApi CurrentContext => _currentContext.Value ?? throw NewInvalidOperationException();

        private static InvalidOperationException NewInvalidOperationException()
            => new( "The 'meta' API can be used only in the execution context of a template." );

        /// <summary>
        /// Injects the logic that has been intercepted. For instance, in an <see cref="OverrideMethodAspect"/>,
        /// calling <see cref="Proceed"/> invokes the method being overridden. Note that the way how the
        /// logic is invoked (as a method call or inlining) is considered an implementation detail.
        /// </summary>
        /// <returns></returns>
        [TemplateKeyword]
        [return: RunTimeOnly]
        public static dynamic? Proceed() => CurrentContext.Proceed() ?? throw NewInvalidOperationException();

        /// <summary>
        /// Requests the debugger to break, if any debugger is attached to the current process.
        /// </summary>
        /// <seealso href="@debugging-aspects"/>
        [TemplateKeyword]
        public static void DebugBreak() => CurrentContext.DebugBreak();

        /// <summary>
        /// Coerces an <paramref name="expression"/> to be interpreted as compile time. This is typically used
        /// to coerce expressions that can be either run-time or compile-time, such as a literal. Since ambiguous expressions are
        /// interpreted as run-time by default, this method allows to change that behavior.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Exactly <paramref name="expression"/>, but coerced as a compile-time expression.</returns>
        /// <seealso href="@templates"/>
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
        /// <seealso href="@templates"/>
        [TemplateKeyword]
        public static T? RunTime<T>( T? value ) => value;

        /// <summary>
        /// Gets the method metadata, or the accessor if this is a template for a field, property or event.
        /// </summary>
        /// <seealso href="@templates"/>
        public static IAdvisedMethod Method => CurrentContext.Method;

        /// <summary>
        /// Gets the target property, or throws an exception if the advice does not target a property.
        /// </summary>
        /// <seealso href="@templates"/>
        public static IAdvisedProperty Property => CurrentContext.Property;

        /// <summary>
        /// Gets the target field or property, or throws an exception if the advice does not target a field or a property.
        /// </summary>
        /// <seealso href="@templates"/>
        public static IAdvisedFieldOrProperty FieldOrProperty => CurrentContext.FieldOrProperty;

        /// <summary>
        /// Gets the target member (method, constructor, field, property or event, but not a nested type), or
        /// throws an exception if the advice does not target member.
        /// </summary>
        /// <seealso href="@templates"/>
        public static IMember Member => CurrentContext.Member;

        /// <summary>
        /// Gets the target event, or throws an exception if the advice does not target an event.
        /// </summary>
        /// <seealso href="@templates"/>
        public static IAdvisedEvent Event => CurrentContext.Event;

        /// <summary>
        /// Gets the list of parameters of the current <see cref="Method"/> or <see cref="Property"/>, or throws an
        /// exception if the advice of the target is neither a method.
        /// </summary>
        /// <seealso href="@templates"/>
        public static IAdvisedParameterList Parameters => CurrentContext.Parameters;

        // Gets the project configuration.
        // IProject Project { get; }

        /// <summary>
        /// Gets the target type of the advice. If the advice is applied to a member, this property returns the declaring
        /// type of the member.
        /// </summary>
        /// <seealso href="@templates"/>
        public static INamedType Type => CurrentContext.Type;

        /// <summary>
        /// Gets the code model of the whole compilation.
        /// </summary>
        /// <seealso href="@templates"/>
        public static ICompilation Compilation => CurrentContext.Compilation;

        /// <summary>
        /// Gets a <c>dynamic</c> object that represents an instance of the target type. It can be used as a value (e.g. as a method argument)
        /// or can be used to get access to <i>instance</i> members of the instance (e.g. <c>meta.This.MyMethod()</c>).
        /// The <see cref="This"/> property exposes the state of the target type as it is <i>after</i> the application
        /// of all aspects. If the member is <c>virtual</c>, a virtual call is performed, therefore the implementation on the child type
        /// (possibly with all applied aspects) is performed.  To access the prior layer (or the base type, if there is no prior layer), use <see cref="Base"/>.
        /// To access static members, use <see cref="ThisStatic"/>.
        /// </summary>
        /// <seealso cref="Base"/>
        /// <seealso cref="ThisStatic"/>
        /// <seealso href="@templates"/>
        [RunTimeOnly]
        public static dynamic This => CurrentContext.This;

        /// <summary>
        /// Gets a <c>dynamic</c> object that must be used to get access to <i>instance</i> members of the instance (e.g. <c>meta.Base.MyMethod()</c>).
        /// The <see cref="Base"/> property exposes the state of the target type as it is <i>before</i> the application
        /// of the current aspect layer. To access the final layer, use <see cref="This"/>.
        /// To access static members, use <see cref="BaseStatic"/>.
        /// </summary>
        /// <seealso cref="This"/>
        /// <seealso cref="BaseStatic"/>
        /// <seealso href="@templates"/>
        [RunTimeOnly]
        public static dynamic Base => CurrentContext.Base;

        /// <summary>
        /// Gets a <c>dynamic</c> object that must be used to get access to <i>static</i> members of the type (e.g. <c>meta.ThisStatic.MyStaticMethod()</c>).
        /// The <see cref="ThisStatic"/> property exposes the state of the target type as it is <i>after</i> the application
        /// of all aspects. To access the prior layer (or the base type, if there is no prior layer), use <see cref="BaseStatic"/>.
        /// To access instance members, use <see cref="This"/>.
        /// </summary>
        /// <seealso cref="This"/>
        /// <seealso cref="BaseStatic"/>
        /// <seealso href="@templates"/>
        [RunTimeOnly]
        public static dynamic ThisStatic => CurrentContext.ThisStatic;

        /// <summary>
        /// Gets a <c>dynamic</c> object that must be used to get access to <i>static</i> members of the type (e.g. <c>meta.BaseStatic.MyStaticMethod()</c>).
        /// The <see cref="BaseStatic"/> property exposes the state of the target type as it is <i>before</i> the application
        /// of the current aspect layer. To access the final layer, use <see cref="ThisStatic"/>.
        /// To access instance members, use <see cref="Base"/>.
        /// </summary>
        /// <seealso cref="Base"/>
        /// <seealso cref="ThisStatic"/>
        /// <seealso href="@templates"/>
        [RunTimeOnly]
        public static dynamic BaseStatic => CurrentContext.BaseStatic;

        /// <summary>
        /// Gets a service allowing to report and suppress diagnostics.
        /// </summary>
        /// <seealso href="@diagnostics"/>
        public static IDiagnosticSink Diagnostics => CurrentContext.Diagnostics;

        /// <summary>
        /// Gets the dictionary of tags that were passed to the <see cref="IAdviceFactory"/> method by the <see cref="IAspect{T}.BuildAspect"/> method.
        /// </summary>
        /// <seealso href="sharing-state-with-advices"/>
        public static IReadOnlyDictionary<string, object?> Tags => CurrentContext.Tags;

        /// <summary>
        /// Gets the list of aspect aspects that have required the current aspect. (Not implemented.)
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

        /// <summary>
        /// Generates the cast syntax for the specified type.  
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value">Must be explicitly cast to <c>object</c> otherwise the C# compiler will emit an error.</param>
        /// <returns></returns>
        /// <seealso href="@templates"/>
        [return: RunTimeOnly]
        [TemplateKeyword]
        public static dynamic? Cast( IType type, dynamic? value ) => type.Compilation.TypeFactory.Cast( type, value );

        internal static IDisposable WithContext( IMetaApi current )
        {
            _currentContext.Value = current;

            return new InitializeCookie();
        }

        private class InitializeCookie : IDisposable
        {
            public void Dispose()
            {
                _currentContext.Value = null;
            }
        }
    }
}