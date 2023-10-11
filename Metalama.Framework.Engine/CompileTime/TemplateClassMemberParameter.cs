﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.CompileTime;

internal sealed record TemplateClassMemberParameter( int SourceIndex, string Name, bool IsCompileTime, int? TemplateIndex );