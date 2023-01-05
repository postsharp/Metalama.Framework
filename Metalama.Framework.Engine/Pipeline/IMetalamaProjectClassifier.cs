﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Pipeline;

public interface IMetalamaProjectClassifier : IGlobalService
{
    bool TryGetMetalamaVersion( Compilation compilation, [NotNullWhen( true )] out Version? version );
}