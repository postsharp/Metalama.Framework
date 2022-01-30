// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// Represents a code action menu, with children items.
    /// </summary>
    [DataContract]
    public class CodeActionMenuModel : CodeActionBaseModel
    {
        [DataMember( Order = NextKey + 0 )]
        public List<CodeActionBaseModel> Items { get; } = new();

        public CodeActionMenuModel( string title ) : base( title ) { }

        public override ImmutableArray<CodeAction> ToCodeActions( CodeActionInvocationContext invocationContext, string titlePrefix = "" )
        {
            if ( invocationContext.HierarchicalItemsSupported )
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
    }
}