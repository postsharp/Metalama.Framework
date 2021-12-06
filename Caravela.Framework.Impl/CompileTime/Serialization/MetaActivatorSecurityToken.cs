// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Security token passed to the <see cref="IMetaActivator.CreateInstance"/> method of the <see cref="IMetaActivator"/> interface.
    /// </summary>
    public sealed class MetaActivatorSecurityToken
    {
        private MetaActivatorSecurityToken() { }

        internal static readonly MetaActivatorSecurityToken Instance = new();
    }
}