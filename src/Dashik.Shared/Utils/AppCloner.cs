namespace Dashik.Shared.Utils;

public sealed class AppCloner : CloneIt
{
    public AppCloner()
    {
        UsePropertyCopy = true;
        CopyEventHandlers = false;
    }

    public static void CloneObjectTo<T>(T source, T destination, bool simpleTypesOnly = false)
    {
        using var cloner = new AppCloner();
        cloner.CloneTo(source, destination, simpleTypesOnly);
    }
}
