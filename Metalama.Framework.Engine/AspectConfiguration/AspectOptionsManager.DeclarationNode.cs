// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;

namespace Metalama.Framework.Engine.AspectConfiguration;

public partial class AspectOptionsManager
{
    private sealed class DeclarationNode
    {
        private readonly ConcurrentLinkedList<DeclarationNode> _children = new();

        public object Sync { get; } = new();

        public Framework.Options.AspectOptions? DirectOptions { get; set; }

        public Framework.Options.AspectOptions? MergedOptions { get; set; }

        public void ResetMergedOptions()
        {
            this.MergedOptions = null;

            foreach ( var child in this._children )
            {
                child.ResetMergedOptions();
            }
        }

        public void AddChildNode( DeclarationNode node )
        {
            this._children.Add( node );
        }
    }
}