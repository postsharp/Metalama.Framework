// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.ArchitectureValidation;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes information about the declaration to which a template was applied.
    /// This interface is exposed by the <see cref="meta"/> static type.
    /// </summary>
    [CompileTimeOnly]
    [InternalImplement]
    internal interface IMetaApi
    {
        IConstructor Constructor { get; }

        IMethodBase MethodBase { get; }

        IField Field { get; }

        IFieldOrProperty FieldOrProperty { get; }

        IDeclaration Declaration { get; }

        IMember Member { get; }

        /// <summary>
        /// Gets the method metadata, or the accessor if this is a template for a field, property or event.
        /// </summary>
        /// <remarks>
        /// To invoke the method, use <see cref="IMethodInvocation.Invoke"/>,
        /// e.g. <c>meta.Method.Invoke(1, 2, 3);</c>.
        /// </remarks>
        IMethod Method { get; }

        /// <summary>
        /// Gets the target field or property, or null if the advice does not target a field or a property.
        /// </summary>
        IProperty Property { get; }

        /// <summary>
        /// Gets the target event, or null if the advice does not target an event.
        /// </summary>
        IEvent Event { get; }

        /// <summary>
        /// Gets the list of parameters of <see cref="Method"/>.
        /// </summary>
        IAdviceParameterList Parameters { get; }

        // Gets the project configuration.
        // IProject Project { get; }

        /// <summary>
        /// Gets the code model of current type including the introductions of the current aspect type.
        /// </summary>
        INamedType Type { get; }

        /// <summary>
        /// Gets the code model of the whole compilation.
        /// </summary>
        ICompilation Compilation { get; }

        /// <summary>
        /// Gets an object that gives <c>dynamic</c> access to the instance members of the type. Equivalent to the <c>this</c> C# keyword.
        /// </summary>
        /// <seealso cref="Base"/>
        dynamic This { get; }

        /// <summary>
        /// Gets an object that gives <c>dynamic</c> access to the instance members of the type in the state they were before the application
        /// of the current advice. Equivalent to the <c>base</c> C# keyword.
        /// </summary>
        /// <seealso cref="This"/>
        dynamic Base { get; }

        dynamic ThisStatic { get; }

        dynamic BaseStatic { get; }

        // Gets the properties that were passed by the aspect initializer.
        IReadOnlyDictionary<string, object?> Tags { get; }

        IDiagnosticSink Diagnostics { get; }
    }
}