// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.CodeModel
{
    internal interface IPropertyImpl : IProperty, IFieldOrPropertyImpl { }

    internal interface IIndexerImpl : IIndexer, IMemberWithAccessorsImpl { }
}