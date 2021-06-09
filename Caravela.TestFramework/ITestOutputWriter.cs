// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    internal interface ITestOutputWriter : ITestOutputHelper, IService { }
}