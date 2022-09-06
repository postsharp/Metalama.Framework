// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Engine.Pipeline
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
        public static readonly ServiceProviderMark Project = new( "Project" );
        internal static readonly ServiceProviderMark Other = new( "Other" );

        // The testing mark is the only public because it is used by the testing API and this is the only use case of this class.
        public static readonly ServiceProviderMark Test = new( "Test" );

        internal void RequireProjectWide()
        {
            if ( this != Project && this != Test )
            {
                // We should get a project-specific service provider here.
                throw new AssertionFailedException();
            }
        }

        public override string ToString() => this._name;
    }
}