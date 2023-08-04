// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Immutable;
using System.IO;

namespace Metalama.Framework.Engine.Options;

/// <summary>
/// A base implementation of <see cref="IProjectOptions"/> that provides default values.
/// </summary>
public class DefaultProjectOptions : IProjectOptions
{
    public virtual string? BuildTouchFile => null;

    public virtual string? SourceGeneratorTouchFile => null;

    public virtual string? AssemblyName => null;

    public virtual bool IsFrameworkEnabled => true;

    public virtual bool FormatOutput => false;

    public virtual bool WriteHtml => false;

    public virtual bool FormatCompileTimeCode => false;

    public virtual bool IsUserCodeTrusted => true;

    public virtual string? ProjectPath => null;

    public virtual string? ProjectName
    {
        get
        {
            var path = this.ProjectPath;

            if ( path == null )
            {
                return null;
            }
            else
            {
                return Path.GetFileNameWithoutExtension( path );
            }
        }
    }

    public virtual string? TargetFramework => "net6.0";

    public virtual string? TargetFrameworkMoniker => throw new NotSupportedException();

    public virtual string? Configuration => "Debug";

    public virtual IProjectOptions Apply( IProjectOptions options ) => options;

    public virtual bool TryGetProperty( string name, out string? value )
    {
        value = null;

        return false;
    }

    public virtual bool RemoveCompileTimeOnlyCode => true;

    public virtual bool RequiresCodeCoverageAnnotations => false;

    public virtual bool AllowPreviewLanguageFeatures => false;

    public virtual bool RequireOrderedAspects => false;

    public virtual bool IsConcurrentBuildEnabled => false;

    public virtual ImmutableArray<string> CompileTimePackages => ImmutableArray<string>.Empty;

    public virtual string? ProjectAssetsFile => null;

    public virtual int? ReferenceAssemblyRestoreTimeout => null;

    public virtual bool IsDesignTimeEnabled => true;

    public virtual string? AdditionalCompilationOutputDirectory => null;

    public virtual string? License => null;

    public virtual bool? WriteLicenseCreditData => null;

    public virtual bool IsTest => false;
}