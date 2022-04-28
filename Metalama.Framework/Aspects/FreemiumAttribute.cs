using System;

namespace Metalama.Framework.Aspects;

/// <summary>
/// Custom attribute that, when applied to an aspect class, means that this aspect class can use all Metalama feature
/// even when running with a Metalama Essentials license. You can use only 1 freemium aspect class per project.
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class FreemiumAttribute : Attribute { }