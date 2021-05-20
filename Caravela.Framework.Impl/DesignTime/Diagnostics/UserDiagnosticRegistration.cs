// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Caravela.Framework.Impl.DesignTime.Diagnostics
{
    /// <summary>
    /// Represents a JSON-serializable user diagnostic for <see cref="UserDiagnosticRegistrationFile"/>.
    /// </summary>
    internal class UserDiagnosticRegistration
    {
        [JsonConstructor]
        public UserDiagnosticRegistration( string id, DiagnosticSeverity severity, string category, string title )
        {
            this.Severity = severity;
            this.Id = id;
            this.Category = category;
            this.Title = title;
        }

        public UserDiagnosticRegistration( DiagnosticDescriptor descriptor )
        {
            this.Id = descriptor.Id;
            this.Severity = descriptor.DefaultSeverity;
            this.Category = descriptor.Category;
            this.Title = "A Caravela user diagnostic.";
        }

        public DiagnosticDescriptor DiagnosticDescriptor() => new( this.Id, this.Title, "", this.Category, this.Severity, true );

        /// <summary>
        /// Gets the severity of the diagnostic.
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets an unique identifier for the diagnostic (e.g. <c>MY001</c>).
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the category of the diagnostic (e.g. your product name).
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Gets a short title describing the diagnostic. This title is typically described in the solution explorer of the IDE
        /// and does not contain formatting string parameters.
        /// </summary>
        public string Title { get; }
    }
}