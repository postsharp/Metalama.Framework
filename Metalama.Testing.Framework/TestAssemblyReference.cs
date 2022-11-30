// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace Metalama.Testing.Framework
{
    /// <summary>
    /// Represents a metadata reference. This class is JSON-serializable.
    /// </summary>
    public class TestAssemblyReference
    {
        public string? Path { get; set; }

        public string? Name { get; set; }

        internal MetadataReference? ToMetadataReference()
        {
            if ( this.Path != null )
            {
                return MetadataReference.CreateFromFile( this.Path! );
            }
            else if ( this.Name != null )
            {
                var assembly = AppDomainUtility.GetLoadedAssemblies( x => string.Equals( x.GetName().Name, this.Name, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();

                if ( assembly == null )
                {
                    assembly = Assembly.Load( this.Name );
                }

                return MetadataReference.CreateFromFile( assembly.Location );
            }

            return null;
        }

        public override string ToString() => this.Path ?? "<null>";
    }
}