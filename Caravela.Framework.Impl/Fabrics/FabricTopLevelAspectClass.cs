// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// The top-level aspect class integrating the fabrics feature in the aspect pipeline. It is used as an 'identity'
    /// class. The real class is <see cref="FabricAggregateAspectClass"/>, which is instantiated in the middle of the pipeline,
    /// while <see cref="FabricTopLevelAspectClass"/> must exist while the pipeline is being instantiated.
    /// </summary>
    internal class FabricTopLevelAspectClass : IAspectClass
    {
        public const string FabricAspectName = "<Fabric>";

        public static readonly IAspectClass Instance = new FabricTopLevelAspectClass();

        string IAspectClass.FullName => FabricAspectName;

        string IAspectClass.DisplayName => FabricAspectName;

        string? IAspectClass.Description => null;

        bool IAspectClass.IsAbstract => false;

        private FabricTopLevelAspectClass() { }
    }
}