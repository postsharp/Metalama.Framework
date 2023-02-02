// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;

namespace Metalama.Framework.Engine.CompileTime.Manifest;

[JsonObject]
internal class TemplateInfoManifest
{
    public TemplateAttributeType AttributeType { get; }

    public bool IsAbstract { get; }

    public TemplateInfoManifest( TemplateAttributeType attributeType, bool isAbstract )
    {
        this.AttributeType = attributeType;
        this.IsAbstract = isAbstract;
    }
}