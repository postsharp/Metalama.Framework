using Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.AspectOnLocalFunction_TopLevelStatements;

[MethodAspect]
[MethodBaseAspect]
[return: Contract]
int LocalFunction([Contract] int a) => a;

LocalFunction(42);