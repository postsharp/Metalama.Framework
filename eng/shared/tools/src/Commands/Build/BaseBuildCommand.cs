using PostSharp.Engineering.BuildTools.Build;
using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public abstract class BaseBuildCommand : Command
    {
        public Product Product { get; }

        protected BaseBuildCommand( Product product,  string name, string? description = null) : base(name, description)
        {
            this.Product = product;
            this.AddOption(
                new Option<BuildConfiguration>( "--configuration", "Build configuration (Debug or Release)" ) );
            this.AddOption( new Option<int>( "--number", "Creates a numbered build" ) );
            this.AddOption( new Option<bool>( "--public", "Creates a public build" ) );
            this.AddOption( new Option<Verbosity>( "--verbosity", "Sets the verbosity" ) );

        }
    }
}