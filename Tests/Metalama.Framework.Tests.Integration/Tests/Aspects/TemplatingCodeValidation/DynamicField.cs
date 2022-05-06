﻿using System.Collections.Generic;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.DynamicField;

public class TheAspect : TypeAspect
{
    private dynamic f1 = null!;
    private dynamic? f2 = null;
    private dynamic[] f3 = null!;
    private List<dynamic> f4 = null!;
}