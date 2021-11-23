using PostSharp.Engineering.BuildTools.Build.Model;
using Spectre.Console.Cli;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace PostSharp.Engineering.BuildTools.Build
{
    public class BaseCommandSettings : CommandSettings
    {
        private string[] _unparsedProperties = Array.Empty<string>();

        [Description( "Lists the additional properties supported by the command" )]
        [CommandOption( "--list-properties" )]
        public bool ListProperties { get; protected set; }

        [Description( "Properties in form Name=Value" )]
        [CommandOption( "-p|--property" )]
        public string[] UnparsedProperties
        {
            get => this._unparsedProperties;

            protected set
            {
                this._unparsedProperties = value;

                this.Properties = this.Properties.AddRange( value.Select( v =>
                {
                    var split = v.Split( '=' );
                    if ( split.Length > 1 )
                    {
                        return new System.Collections.Generic.KeyValuePair<string, string>( split[0].Trim(), split[1].Trim() );
                    }
                    else
                    {
                        return new System.Collections.Generic.KeyValuePair<string, string>( split[0].Trim(), "True" );
                    }
                } ) );
            }
        }



        public ImmutableDictionary<string, string> Properties { get; protected set; } =
            ImmutableDictionary.Create<string, string>( StringComparer.OrdinalIgnoreCase );
    }
    public class BaseBuildSettings : BaseCommandSettings
    {
       

        [Description( "Sets the build configuration (Debug or Release)" )]
        [CommandOption( "-c|--configuration" )]
        public BuildConfiguration BuildConfiguration { get; protected set; }

        [Description( "Creates a numbered build (typically for an internal CI build)" )]
        [CommandOption( "--numbered" )]
        public int BuildNumber { get; protected set; }

        [Description( "Creates a public build (typically to publish to nuget.org)" )]
        [CommandOption( "--public" )]
        public bool PublicBuild { get; protected set; }

        [Description( "Sets the verbosity" )]
        [CommandOption( "-v|--verbosity" )]
        [DefaultValue( Verbosity.Minimal )]
        public Verbosity Verbosity { get; protected set; }

        [Description( "Executes only the current command, but not the previous command" )]
        [CommandOption( "--no-dependencies" )]
        public bool NoDependencies { get; protected set; }

        [Description( "Determines wether test-only assemblies should be included in the operation" )]
        [CommandOption( "--include-tests" )]
        public bool IncludeTests { get; protected set; }

        [Description( "Disables concurrent processing" )]
        [CommandOption( "--no-concurrency" )]
        public bool NoConcurrency { get; protected set; }

        [Description( "Use force" )]
        [CommandOption( "--force" )]
        public bool Force { get; protected set; }

      

        public BaseBuildSettings WithIncludeTests( bool value )
        {
            var clone = (BaseBuildSettings) this.MemberwiseClone();
            clone.IncludeTests = value;

            return clone;
        }

        public BaseBuildSettings WithoutConcurrency()
        {
            var clone = (BaseBuildSettings) this.MemberwiseClone();
            clone.NoConcurrency = true;

            return clone;
        }


        public BaseBuildSettings WithAdditionalProperties( ImmutableDictionary<string, string> properties )
        {
            if ( properties.IsEmpty )
            {
                return this;
            }

            var clone = (BaseBuildSettings) this.MemberwiseClone();
            clone.Properties = clone.Properties.AddRange( properties );

            return clone;
        }


        public VersionSpec VersionSpec => this.BuildNumber > 0
            ? new VersionSpec( VersionKind.Numbered, this.BuildNumber )
            : this.PublicBuild
                ? new VersionSpec( VersionKind.Public )
                : new VersionSpec( VersionKind.Local );
    }
}