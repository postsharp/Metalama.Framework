// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes;

internal class CodeActionMenuBuilder
{
    private readonly CodeActionMenuModel _rootMenu = new( "<root>" );

    public void AddItem( string title, Func<string, CodeActionBaseModel> createItem )
    {
        var titleParts = title.Split( '|' );

        // Get or create the top-level item.

        var menu = GetOrCreateMenu( titleParts.Length - 2 );

        if ( menu != null )
        {
            menu.Items.Add( createItem( titleParts[^1].Trim() ) );
        }

        CodeActionMenuModel? GetOrCreateMenu( int level )
        {
            if ( level == -1 )
            {
                return this._rootMenu;
            }
            else
            {
                var parentMenu = GetOrCreateMenu( level - 1 );

                if ( parentMenu == null )
                {
                    // Could not find or create the parent menu.
                    return null;
                }

                var titleOfThisLevel = titleParts[level].Trim();
                var item = parentMenu.Items.FirstOrDefault( i => i.Title == titleOfThisLevel );

                switch ( item )
                {
                    case null:
                        {
                            var childMenu = new CodeActionMenuModel( titleOfThisLevel );
                            parentMenu.Items.Add( childMenu );

                            return childMenu;
                        }

                    case CodeActionMenuModel childMenu:
                        return childMenu;

                    default:
                        // There is a conflict, and we don't try to resolve it.
                        return null;
                }
            }
        }
    }

    public ImmutableArray<CodeActionBaseModel> Build()
    {
        this._rootMenu.Sort();

        return this._rootMenu.Items.ToImmutableArray();
    }
}