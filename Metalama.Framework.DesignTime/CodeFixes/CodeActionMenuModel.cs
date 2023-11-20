// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a code action menu, with children items.
    /// </summary>
    public sealed class CodeActionMenuModel : CodeActionBaseModel
    {
        public List<CodeActionBaseModel> Items { get; } = [];

        public CodeActionMenuModel( string title ) : base( title ) { }

        public override ImmutableArray<CodeAction> ToCodeActions( CodeActionInvocationContext invocationContext, string titlePrefix = "" )
        {
            if ( CodeActionInvocationContext.HierarchicalItemsSupported )
            {
                // If the IDE supports hierarchical items, we just reproduce the structure.

                var codeActions = this.Items.SelectMany( i => i.ToCodeActions( invocationContext, titlePrefix ) ).ToImmutableArray();

                if ( codeActions.IsDefaultOrEmpty )
                {
                    return ImmutableArray<CodeAction>.Empty;
                }
                else
                {
                    return ImmutableArray.Create( CodeAction.Create( this.Title, codeActions, false ) );
                }
            }
            else
            {
                // If the IDE does NOT support hierarchical items, we have to flatten the menu structure and recursively add title prefixes.

                var codeActions = new List<CodeAction>();

                void ProcessMenu( CodeActionMenuModel menu, string prefix )
                {
                    var childPrefix = prefix.Length == 0 ? menu.Title + TitleJoin : prefix + TitleJoin + menu.Title + TitleJoin;

                    foreach ( var item in menu.Items )
                    {
                        switch ( item )
                        {
                            case CodeActionModel codeAction:
                                codeActions.AddRange( codeAction.ToCodeActions( invocationContext, childPrefix ) );

                                break;

                            case CodeActionMenuModel childMenu:
                                ProcessMenu( childMenu, childPrefix );

                                break;
                        }
                    }
                }

                ProcessMenu( this, "" );

                return codeActions.ToImmutableArray();
            }
        }

        public void Sort()
        {
            this.Items.Sort( ( x, y ) => string.Compare( x.Title, y.Title, StringComparison.Ordinal ) );

            foreach ( var item in this.Items )
            {
                if ( item is CodeActionMenuModel menu )
                {
                    menu.Sort();
                }
            }
        }
    }
}