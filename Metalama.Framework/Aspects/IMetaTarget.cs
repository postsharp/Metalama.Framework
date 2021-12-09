// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Aspects
{
    [CompileTimeOnly]
    [InternalImplement]
    public interface IMetaTarget
    {
        [Obsolete( "Not implemented." )]
        IConstructor Constructor { get; }

        /// <summary>
        /// Gets the method metadata, or the accessor if this is a template for a field, property or event.
        /// </summary>
        /// <seealso href="@templates"/>
        IMethodBase MethodBase { get; }

        IAdvisedField Field { get; }

        /// <summary>
        /// Gets the target field or property, or throws an exception if the advice does not target a field or a property.
        /// </summary>
        /// <seealso href="@templates"/>
        IAdvisedFieldOrProperty FieldOrProperty { get; }

        IDeclaration Declaration { get; }

        /// <summary>
        /// Gets the target member (method, constructor, field, property or event, but not a nested type), or
        /// throws an exception if the advice does not target member.
        /// </summary>
        /// <seealso href="@templates"/>
        IMember Member { get; }

        /// <summary>
        /// Gets the method metadata, or the accessor if this is a template for a field, property or event.
        /// </summary>
        /// <remarks>
        /// To invoke the method, use <see cref="IMethodInvoker.Invoke"/>,
        /// e.g. <c>meta.Target.Method.Invoke(1, 2, 3);</c>.
        /// </remarks>
        IAdvisedMethod Method { get; }

        /// <summary>
        /// Gets the target field or property, or null if the advice does not target a field or a property.
        /// </summary>
        IAdvisedProperty Property { get; }

        /// <summary>
        /// Gets the target event, or null if the advice does not target an event.
        /// </summary>
        IAdvisedEvent Event { get; }

        /// <summary>
        /// Gets the list of parameters of <see cref="Method"/>.
        /// </summary>
        IAdvisedParameterList Parameters { get; }

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

        IProject Project { get; }
    }
}