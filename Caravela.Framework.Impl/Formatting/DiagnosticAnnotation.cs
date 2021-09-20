// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable 8618 // Property Id not initialized.

namespace Caravela.Framework.Impl.Formatting
{
    public class DiagnosticAnnotation
    {
        [JsonConstructor]
        public DiagnosticAnnotation() { }

        public DiagnosticAnnotation( Diagnostic diagnostic )
        {
            this.Id = diagnostic.Id;
            this.Message = diagnostic.GetMessage();
            this.Severity = diagnostic.Severity;
        }

        public string Id { get; init; }

        public DiagnosticSeverity Severity { get; init; }

        public string Message { get; init; }

        public string ToJson() => JsonConvert.SerializeObject( this );

        public static DiagnosticAnnotation FromJson( string json ) => JsonConvert.DeserializeObject<DiagnosticAnnotation>( json )!;

        public override string ToString() => $"{this.Severity} {this.Id}: {this.Message}";
    }
}