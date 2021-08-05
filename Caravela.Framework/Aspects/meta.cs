// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedParameter.Global
// ReSharper disable once InconsistentNaming
namespace Caravela.Framework.Aspects
{
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

        private static NotSupportedException NewMustBeTransformedException( [CallerMemberName] string? caller = null )
            => new( $"Calls to {caller} are supposed to be transformed." );

        /// <summary>
        /// Gets access to the declaration being overridden or introduced.
        /// </summary>
        public static IMetaTarget Target => CurrentContext.Target;

        /// <summary>
        /// Invokes the logic that has been overwritten. For instance, in an <see cref="OverrideMethodAspect"/>,
        /// calling <see cref="Proceed"/> invokes the method being overridden. Note that the way how the
        /// logic is invoked (as a method call or inlining) is considered an implementation detail.
        /// </summary>
        [TemplateKeyword]
        public static dynamic? Proceed() => throw NewMustBeTransformedException();

        /// <summary>
        /// Synonym to <see cref="Proceed"/>, but the return type is exposed as a <c>Task&lt;dynamic?&gt;</c>.
        /// Only use this method when the return type of the method or accessor is task-like. Note that
        /// the actual return type of the overridden method or accessor is the one of the overwritten semantic, so it
        /// can be a void <see cref="Task"/>, a <see cref="ValueType"/>, or any other type.
        /// </summary>
        public static Task<dynamic?> ProceedAsync() => throw NewMustBeTransformedException();

        /// <summary>
        /// Synonym to <see cref="Proceed"/>, but the return type is exposed as a <c>IEnumerable&lt;dynamic?&gt;</c>.
        /// </summary>
        public static IEnumerable<dynamic?> ProceedEnumerable() => throw NewMustBeTransformedException();

        /// <summary>
        /// Synonym to <see cref="Proceed"/>, but the return type is exposed as a <c>IEnumerator&lt;dynamic?&gt;</c>.
        /// </summary>
        public static IEnumerator<dynamic?> ProceedEnumerator() => throw NewMustBeTransformedException();

#if NET5_0
        /// <summary>
        /// Synonym to <see cref="Proceed"/>, but the return type is exposed as a <c>IAsyncEnumerable&lt;dynamic?&gt;</c>.
        /// </summary>
        public static IAsyncEnumerable<dynamic?> ProceedAsyncEnumerable() => throw NewMustBeTransformedException();

        /// <summary>
        /// Synonym to <see cref="Proceed"/>, but the return type is exposed as a <c>IAsyncEnumerator&lt;dynamic?&gt;</c>.
        /// </summary>
        public static IAsyncEnumerator<dynamic?> ProceedAsyncEnumerator() => throw NewMustBeTransformedException();
#endif

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
        [return: CompileTimeOnly]
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
        [return: NotNullIfNotNull( "value" )]
        public static T? RunTime<T>( T? value ) => value;

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
        [TemplateKeyword]
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
        [TemplateKeyword]
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
        [TemplateKeyword]
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
        [TemplateKeyword]
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