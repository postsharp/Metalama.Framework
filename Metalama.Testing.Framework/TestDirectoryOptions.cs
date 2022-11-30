// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;
using System.IO;

namespace Metalama.Testing.Framework
{
    /// <summary>
    /// Represent the content of the <c>metalamaTests.json</c> file. This class is JSON-serializable.
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

        internal override void ApplyBaseOptions( TestDirectoryOptions baseOptions )
        {
            base.ApplyBaseOptions( baseOptions );

            this.Exclude ??= baseOptions.Exclude;
        }
    }
}