using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.DeclarativeAdviceWithTemplateInitialization;
#pragma warning disable CS0067

internal class BuildInfo
{
#pragma warning disable CS0067
    private class Fabric : TypeFabric
    {
        [Introduce]
        public string? TargetFramework { get; } = meta.Target.Project.TargetFramework;

        [Introduce]
        public string? Configuration { get; } = meta.Target.Project.Configuration;
    }
#pragma warning restore CS0067


    private global::System.String? _configuration = "Debug";


    public global::System.String? Configuration 
    { get
        { 
            return this._configuration;
        }
    } 

    private global::System.String? _targetFramework = "net6.0";


    public global::System.String? TargetFramework 
    { get
        { 
            return this._targetFramework;
        }
    } }
#pragma warning restore CS0067
    