class TargetCode
{
// Rewritten.
    [Aspect]
    int TransformedMethod( int a ) => 0;
        
    int NotTransformedMethod( int a ) => 0;
}