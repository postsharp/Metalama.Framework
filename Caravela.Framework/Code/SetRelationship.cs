// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    internal struct SetRelationship
    {
        private readonly int? _comparison;

        public SetRelationship(int? comparison)
        {
            this._comparison = comparison;
        }

        public bool IsEqual => this._comparison == 0;

        public bool IsSubset => this._comparison < 0;

        public bool IsSuperset => this._comparison > 0;

        public bool IsSubsetOrEqual => this._comparison <= 0;

        public bool IsSupersetOrEqual => this._comparison >= 0;
    }
}
