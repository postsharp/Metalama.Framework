using System.Collections.Generic;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Represents a lexical scope which allows the template method to define new unique identifiers within the target code element.
    /// </summary>
    internal interface ITemplateExpansionLexicalScope
    {
        /// <summary>
        /// Gets parent scope.
        /// </summary>
        ITemplateExpansionLexicalScope? Parent { get; }

        /// <summary>
        /// Gets a map of identifiers defined directly in this scope (excluding parent scope and nested scopes).
        /// </summary>
        IReadOnlyCollection<string> DefinedIdentifiers { get; }

        /// <summary>
        /// Gets a list of nested scopes defined by this lexical scope.
        /// </summary>
        IReadOnlyList<ITemplateExpansionLexicalScope> NestedScopes { get; }

        /// <summary>
        /// Determines whether a given name is defined in this scope.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="includeAncestorScopes">True if lookup should include ancestor scopes.</param>
        /// <returns></returns>
        bool IsDefined( string name, bool includeAncestorScopes = true );

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

    internal static class TemplateExpansionLexicalScopeExtensions
    {
        /// <summary>
        /// Determines whether a name can be defined in the scope. The name should not collide with any name in ancestors and nested scopes.
        /// </summary>
        /// <param name="scope">Scope.</param>
        /// <param name="name">Name.</param>
        /// <returns>True.</returns>
        public static bool IsDefineable( this ITemplateExpansionLexicalScope scope, string name )
        {
            return scope.IsDefined( name, true ) || IsDefinedInNestedScope( scope, name );

            static bool IsDefinedInNestedScope( ITemplateExpansionLexicalScope scope, string name )
            {
                foreach ( var nestedScope in scope.NestedScopes )
                {
                    if ( nestedScope.IsDefined( name, false ) )
                    {
                        return true;
                    }

                    if ( IsDefinedInNestedScope( nestedScope, name ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
