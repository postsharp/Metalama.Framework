﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Metalama.Framework.Engine.Licensing;

[JsonObject]
public sealed class LicenseConsumptionFile
{
    public string ProjectPath { get; }

    public string Configuration { get; }

    public string TargetFramework { get; }

    public int TotalAspectClasses { get; }

    public string MetalamaVersion { get; }

    public DateTime? MetalamaBuildDate { get; }

    public IReadOnlyList<string> ConsumedAspectClasses { get; }

    [JsonIgnore]
    public DateTime BuildTime { get; private set; } = DateTime.Now;

    [JsonIgnore]
    public string? DataFilePath { get; private set; }

    public LicenseConsumptionFile(
        string projectPath,
        string configuration,
        string targetFramework,
        int totalAspectClasses,
        IReadOnlyList<string> consumedAspectClasses,
        string metalamaVersion,
        DateTime? metalamaBuildDate )
    {
        this.ProjectPath = projectPath;
        this.Configuration = configuration;
        this.TargetFramework = targetFramework;
        this.ConsumedAspectClasses = consumedAspectClasses;
        this.MetalamaVersion = metalamaVersion;
        this.MetalamaBuildDate = metalamaBuildDate;
        this.TotalAspectClasses = totalAspectClasses;
    }

    public string GetFileName()
    {
        var uniqueId = $"{this.ProjectPath}-{this.Configuration}-{this.TargetFramework}";
        var hash = HashUtilities.HashString( uniqueId );

        return $"{LicenseVerifier.LicenseUsageFilePrefix}{Path.GetFileNameWithoutExtension( this.ProjectPath )}-{hash}.json";
    }

    public void WriteToDirectory( string directory )
    {
        var json = JsonConvert.SerializeObject( this );
        var path = Path.Combine( directory, this.GetFileName() );

        using ( MutexHelper.WithGlobalLock( path ) )
        {
            File.WriteAllText( path, json );
        }
    }

    public static LicenseConsumptionFile? FromFile( string path )
    {
        var json = File.ReadAllText( path );

        var obj = JsonConvert.DeserializeObject<LicenseConsumptionFile>( json );

        if ( obj == null )
        {
            return null;
        }

        obj.BuildTime = File.GetLastWriteTime( path );
        obj.DataFilePath = path;

        return obj;
    }
}