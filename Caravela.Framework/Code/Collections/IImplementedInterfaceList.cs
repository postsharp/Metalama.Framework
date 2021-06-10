﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Collections
{
    public interface IImplementedInterfaceList : IReadOnlyList<INamedType>
    {
        /// <summary>
        /// Determines whether the current collection contains a given <see cref="Type"/>.
        /// </summary>
        bool Contains( Type type );
    }
}