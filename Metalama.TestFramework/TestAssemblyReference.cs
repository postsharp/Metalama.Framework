// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace Metalama.TestFramework
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
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault( x => string.Equals( x.GetName().Name, this.Name, StringComparison.OrdinalIgnoreCase ) );

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