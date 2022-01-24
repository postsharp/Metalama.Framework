[Introduction]
internal class TargetClass
{

    public global::System.String IntroducedField = Initialize_IntroducedField();

    private static global::System.String Initialize_IntroducedField()
    {
        return "IntroducedField";
    }

    public static global::System.String IntroducedField_Static = Initialize_IntroducedField_Static();

    private static global::System.String Initialize_IntroducedField_Static()
    {
        return "IntroducedField_Static";
    }
}