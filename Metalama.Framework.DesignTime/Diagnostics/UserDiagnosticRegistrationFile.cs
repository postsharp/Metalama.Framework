// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using System.Reflection;

namespace Metalama.Framework.DesignTime.Diagnostics
{
    /// <summary>
    /// A JSON-serializable file that contains user-defined diagnostic and suppression descriptors.
    /// </summary>
    [Obfuscation( Exclude = true /* JSON */ )]
    [ConfigurationFile( "userDiagnostics.json" )]
    internal class UserDiagnosticRegistrationFile : ConfigurationFile
    {
        public Dictionary<string, UserDiagnosticRegistration> Diagnostics { get; } = new( StringComparer.OrdinalIgnoreCase );

        public HashSet<string> Suppressions { get; } = new( StringComparer.OrdinalIgnoreCase );

        public override void CopyFrom( ConfigurationFile configurationFile )
        {
            var source = (UserDiagnosticRegistrationFile) configurationFile;

            this.Diagnostics.Clear();
            this.Suppressions.Clear();

            foreach ( var diagnostic in source.Diagnostics )
            {
                this.Diagnostics.Add( diagnostic.Key, diagnostic.Value );
            }

            foreach ( var suppression in source.Suppressions )
            {
                this.Suppressions.Add( suppression );
            }
        }
    }
}