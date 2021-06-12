// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using System.IO;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represent the content of the <c>caravelaTests.json</c> file. This class is JSON-serializable.
    /// </summary>
    public class TestDirectoryOptions : TestOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current directory and all child directories should be excluded.
        /// </summary>
        public bool? Exclude { get; set; }
        
        internal static TestDirectoryOptions ReadFile( string path )
        {
            var json = File.ReadAllText( path );
            
            return JsonConvert.DeserializeObject<TestDirectoryOptions>( json )!;
        }

        internal override void ApplyDirectoryOptions( TestDirectoryOptions directoryOptions )
        {
            base.ApplyDirectoryOptions( directoryOptions );

            if ( this.Exclude == null )
            {
                this.Exclude = directoryOptions.Exclude;
            }
        }
    }
}