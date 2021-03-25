﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    internal class MemberAnalysisResult
    {
        public bool HasSimpleReturns { get; }

        public MemberAnalysisResult( bool hasSimpleBody )
        {
            this.HasSimpleReturns = hasSimpleBody;
        }
    }
}
