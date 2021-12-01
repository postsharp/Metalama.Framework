// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Security token passed to the <see cref="IMetaActivator.CreateInstance"/> method of the <see cref="IMetaActivator"/> interface.
    /// </summary>
    public sealed class MetaActivatorSecurityToken
    {
        private MetaActivatorSecurityToken() { }

        internal static readonly MetaActivatorSecurityToken Instance = new MetaActivatorSecurityToken();
    }
}