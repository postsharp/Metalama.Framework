// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    private sealed class AuxiliaryMemberTransformations
    {
        // Indicates that source declaration has to be injected. This declaration will receive inserted statements and initializers.
        // Used for primary constructors.
        private volatile bool _shouldInjectSourceVersion;

        public bool ShouldInjectSourceVersion => this._shouldInjectSourceVersion;

        public void InjectSourceVersion() => this._shouldInjectSourceVersion = true;
    }
}