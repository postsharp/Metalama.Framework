﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal record struct PartialTypeDependency( TypeDependencyKey MasterType, string DependentFilePath );