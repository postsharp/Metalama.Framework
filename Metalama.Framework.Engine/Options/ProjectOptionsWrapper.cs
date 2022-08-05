﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Options;

/// <summary>
/// An implementation of <see cref="IProjectOptions"/> that delegates all properties and methods to another <see cref="IProjectOptions"/>.
/// All members are virtual and at least one must be overridden.
/// </summary>
public abstract class ProjectOptionsWrapper : IProjectOptions
{
    protected IProjectOptions Wrapped { get; }

    protected ProjectOptionsWrapper( IProjectOptions wrapped )
    {
        this.Wrapped = wrapped;
    }

    public virtual string? BuildTouchFile => this.Wrapped.BuildTouchFile;

    public virtual string? SourceGeneratorTouchFile => this.Wrapped.SourceGeneratorTouchFile;

    public virtual string? AssemblyName => this.Wrapped.AssemblyName;

    public virtual ImmutableArray<object> PlugIns => this.Wrapped.PlugIns;

    public virtual bool IsFrameworkEnabled => this.Wrapped.IsFrameworkEnabled;

    public virtual bool FormatOutput => this.Wrapped.FormatOutput;

    public virtual bool FormatCompileTimeCode => this.Wrapped.FormatCompileTimeCode;

    public virtual bool IsUserCodeTrusted => this.Wrapped.IsUserCodeTrusted;

    public virtual string? ProjectPath => this.Wrapped.ProjectPath;

    public virtual string? TargetFramework => this.Wrapped.TargetFramework;

    public virtual string? Configuration => this.Wrapped.Configuration;

    public virtual bool IsDesignTimeEnabled => this.Wrapped.IsDesignTimeEnabled;

    public virtual string? AdditionalCompilationOutputDirectory => this.Wrapped.AdditionalCompilationOutputDirectory;

    public virtual IProjectOptions Apply( IProjectOptions options ) => this.Wrapped.Apply( options );

    public virtual bool TryGetProperty( string name, out string? value ) => this.Wrapped.TryGetProperty( name, out value );

    public virtual bool RemoveCompileTimeOnlyCode => this.Wrapped.RemoveCompileTimeOnlyCode;

    public virtual bool RequiresCodeCoverageAnnotations => this.Wrapped.RequiresCodeCoverageAnnotations;

    public bool AllowPreviewLanguageFeatures => this.Wrapped.AllowPreviewLanguageFeatures;
}