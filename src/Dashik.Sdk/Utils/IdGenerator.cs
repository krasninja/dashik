namespace Dashik.Sdk.Utils;

/// <summary>
/// Generates identifiers.
/// </summary>
public static class IdGenerator
{
    private static readonly char[] _identifierCharacters =
    [
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L',
        'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
    ];

    /// <summary>
    /// Generate random letters string.
    /// </summary>
    /// <param name="prefix">Prefix.</param>
    /// <param name="length">String length.</param>
    /// <returns>Random length string.</returns>
    public static string Generate(string? prefix = null, int length = 12)
    {
        var randomChars = string.Join(
            string.Empty,
            Random.Shared.GetItems(_identifierCharacters, length));
        if (!string.IsNullOrEmpty(prefix))
        {
            return prefix + '-' + randomChars;
        }
        return randomChars;
    }
}
