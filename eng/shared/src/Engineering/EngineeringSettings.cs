using PostSharp.Engineering.BuildTools.Build;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PostSharp.Engineering.BuildTools.Engineering
{
    internal class EngineeringSettings : BaseCommandSettings
    {
        [Description( "Allows to overwrite the target local repo even when there are uncommitted changes." )]
        [CommandOption( "--force" )]
        public bool Force { get; init; }

        [Description( "Clones the repo if it does not exist." )]
        [CommandOption( "--create" )]
        public bool Create { get; init; }

        [Description( "Remote URL of the repo." )]
        [CommandOption( "-u|--url" )]
        public string Url { get; init; } = "https://postsharp@dev.azure.com/postsharp/Caravela/_git/Caravela.Engineering";
    }

    internal class PullEngineeringSettings : EngineeringSettings
    {
        [Description( "Name of the branch. The default is 'develop' for push and 'master' for pull." )]
        [CommandOption( "-b|--branch" )]
        public string? Branch { get; init; }

       


    }
}
