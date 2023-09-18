﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Options;

namespace Metalama.Framework.Engine.HierarchicalOptions;

public sealed partial class HierarchicalOptionsManager
{
    private sealed class DeclarationNode
    {
        private readonly ConcurrentLinkedList<DeclarationNode> _children = new();

        public object Sync { get; } = new();
        
        public IHierarchicalOptions? DirectOptions { get; set; }

        public IHierarchicalOptions? MergedOptions { get; set; }
        
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