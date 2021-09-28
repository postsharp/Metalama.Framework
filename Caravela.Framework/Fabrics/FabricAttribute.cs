// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Fabrics
{
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class FabricAttribute : Attribute
    {
        /// <summary>
        /// Gets the identifier of the fabric type in the source code.
        /// </summary>
        public string Id { get; }

        public FabricAttribute( string id, string targetId, string path )
        {
            this.Id = id;
            this.TargetId = targetId;
            this.Path = path;
        }

        /// <summary>
        /// Gets the identifier of the target declaration of the fabric.
        /// </summary>
        public string TargetId { get; }

        /// <summary>
        /// Gets the path of the source file in which the fabric is defined.
        /// </summary>
        public string Path { get; }
    }
}