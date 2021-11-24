// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using PostSharp.Backstage.Extensibility;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A diagnostic mark that can be added to a <see cref="ServiceProvider"/> using the <see cref="ServiceProvider.WithMark"/> method.
    /// It is used only for testing, diagnostic and assertion checks.
    /// </summary>
    public sealed class ServiceProviderMark : IService
    {
        private readonly string _name;

        private ServiceProviderMark( string name )
        {
            this._name = name;
        }

        internal static readonly ServiceProviderMark Global = new( "Global" );
        internal static readonly ServiceProviderMark AsyncLocal = new( "AsyncLocal" );
        internal static readonly ServiceProviderMark Pipeline = new( "Pipeline" );
        internal static readonly ServiceProviderMark Project = new( "Project" );
        internal static readonly ServiceProviderMark Other = new( "Other" );

        // The testing mark is the only public because it is used by the testing API and this is the only use case of this class.
        public static readonly ServiceProviderMark Test = new( "Test" );

        public override string ToString() => this._name;
    }
}