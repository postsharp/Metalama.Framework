// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Represents a lexical scope which allows the template method to define new unique identifiers within the target code element.
    /// </summary>
    public interface ITemplateExpansionLexicalScope : IDisposable
    {
        /// <summary>
        /// Creates a new unique identifier name based on the given name.
        /// Calling repeatedly with the same <paramref name="name"/> value will create a new unique identifier each time.
        /// </summary>
        /// <param name="name">A name of the identifier that is used as a base for a new unique name.</param>
        /// <returns>
        /// A new identifer name that is guaranteed to be unique in the current lexical scope.
        /// </returns>
        string DefineIdentifier( string name );

        /// <summary>
        /// Finds an existing identifier name within this lexical scope which corresponds to the given name.
        /// If an identifier with the given name has not been defined then <paramref name="name"/> is returned as is.
        /// If an identifier with the given name has been defined multiple times then the latest defined value is returned.
        /// </summary>
        /// <param name="name">The original name of the identifier that is used to look up the translated name.</param>
        /// <returns>
        /// An identifier name in the current lexical scope.
        /// </returns>
        string LookupIdentifier( string name );

        /// <summary>
        /// Opens a new lexical scope which is nested within the current scope.
        /// </summary>
        /// <returns>
        /// A new lexical scope instance which is nested within the current one.
        /// </returns>
        ITemplateExpansionLexicalScope OpenNestedScope();
    }
}
