// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Custom attribute added by <see cref="CompileTimeCompilationBuilder"/> to the compile-time assembly. It stores original
    /// path of the source file that contained the declaration (typically a fabric). This is used to order the fabrics by depth
    /// of directory.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Interface )]
    public sealed class OriginalPathAttribute : Attribute
    {
        public string Path { get; }

        public OriginalPathAttribute( string path )
        {
            this.Path = path;
        }
    }
}