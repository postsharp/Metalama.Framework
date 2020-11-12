using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Patterns.Costura
{
    public static class Configuration
    {
        public static CosturaAspect Read(AttributeData attributeData, IDiagnosticSink diagnosticSink)
        {
            CosturaAspect config = new CosturaAspect();
            var namedArguments = attributeData.NamedArguments;

            config.DisableCleanup = namedArguments.GetSafeBool(nameof(CosturaAspect.DisableCleanup), false);
            config.DisableCompression = namedArguments.GetSafeBool(nameof(CosturaAspect.DisableCompression), false);
            config.IncludeDebugSymbols = namedArguments.GetSafeBool(nameof(CosturaAspect.IncludeDebugSymbols), true);
            config.CreateTemporaryAssemblies = namedArguments.GetSafeBool(nameof(CosturaAspect.CreateTemporaryAssemblies), false);
            config.IgnoreSatelliteAssemblies = namedArguments.GetSafeBool(nameof(CosturaAspect.IgnoreSatelliteAssemblies), false);
            
            config.IncludeAssemblies = namedArguments.GetSafeStringArray(nameof(CosturaAspect.IncludeAssemblies));
            config.ExcludeAssemblies = namedArguments.GetSafeStringArray(nameof(CosturaAspect.ExcludeAssemblies));
            config.PreloadOrder = namedArguments.GetSafeStringArray(nameof(CosturaAspect.PreloadOrder));
            config.Unmanaged32Assemblies = namedArguments.GetSafeStringArray(nameof(CosturaAspect.Unmanaged32Assemblies));
            config.Unmanaged64Assemblies = namedArguments.GetSafeStringArray(nameof(CosturaAspect.Unmanaged64Assemblies));
            
            if (config.IncludeAssemblies != null && config.IncludeAssemblies.Length > 0 &&
                config.ExcludeAssemblies != null && config.ExcludeAssemblies.Length > 0)
            {
                var syntaxReference = attributeData.ApplicationSyntaxReference;
                diagnosticSink.AddDiagnostic(Diagnostic.Create(
                    "CO002", "Caravela.Patterns.Costura", "Set IncludeAssemblies, or ExcludeAssemblies, but not both.", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0,
                    location: Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span)));
            }
            return config;
        }
    }
}