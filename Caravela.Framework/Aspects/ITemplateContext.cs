// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes information about the element of code to which a template was applied.
    /// This interface is exposed by the <see cref="TemplateContext.target"/> member.
    /// </summary>
    [CompileTimeOnly]
    public interface ITemplateContext : IDiagnosticSink
    {
        /// <summary>
        /// Gets the method metadata, or the accessor if this is a template for a field, property or event.
        /// </summary>
        /// <remarks>
        /// To invoke the method, use <c>Invoke</c>.
        /// e.g. <c>OverrideMethodContext.Method.Invoke(1, 2, 3);</c>.
        /// </remarks>
        IMethod Method { get; }

        /*
        /// <summary>
        /// Gets the target field or property, or null if the advice does not target a field or a property.
        /// </summary>
        IProperty? Property { get; }

        /// <summary>
        /// Gets the target event, or null if the advice does not target an event.
        /// </summary>
        IEvent? Event { get; }
*/

        /// <summary>
        /// Gets the list of parameters of <see cref="Method"/>.
        /// </summary>
        IAdviceParameterList Parameters { get; }

        // Gets the project configuration.
        // IProject Project { get; }

        /// <summary>
        /// Gets the code model of current type including the introductions of the current aspect type.
        /// </summary>
        IType Type { get; }

        /// <summary>
        /// Gets the code model of the whole compilation.
        /// </summary>
        ICompilation Compilation { get; }

        /// <summary>
        /// Gets an object that gives access to the current type including members introduced by the current aspect.
        /// Both instance and static members are made accessible. For instance members,
        /// the <c>this</c> instance is assumed.
        /// </summary>
        dynamic This { get; }

        /*
        /// <summary>
        /// Gives access to the current type in the state it was before the current aspect.
        /// </summary>
        //dynamic Base { get; }

        // Gets the properties that were passed by the aspect initializer.
        //IReadOnlyDictionary<string, object> Properties { get; }
        */
    }
}
