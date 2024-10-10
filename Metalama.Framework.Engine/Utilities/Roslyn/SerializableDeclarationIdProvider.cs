// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using MetalamaMethodKind = Metalama.Framework.Code.MethodKind;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static partial class SerializableDeclarationIdProvider
{
    private const string _assemblyPrefix = "Assembly:";

    private static readonly char[] _separators = [';', '='];
}