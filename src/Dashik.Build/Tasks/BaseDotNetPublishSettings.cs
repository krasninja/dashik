using System.Text;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core.IO.Arguments;

namespace Dashik.Build.Tasks;

public sealed class BaseDotNetPublishSettings : DotNetPublishSettings
{
    public BaseDotNetPublishSettings(BuildContext context, string? properties = null)
    {
        properties ??= string.Empty;

        Force = true;
        PublishSingleFile = true;
        EnableCompressionInSingleFile = true;
        Configuration = DotNetConstants.ConfigurationRelease;
        OutputDirectory = context.OutputDirectory;
        NoLogo = true;
        ArgumentCustomization = pag =>
        {
            // For reference: https://andrewlock.net/version-vs-versionsuffix-vs-packageversion-what-do-they-all-mean/.
            pag.Append(new TextArgument($"-p:InformationalVersion={context.Version}"));

            foreach (var keyValue in GetKeyValuesFromString(properties))
            {
                pag.Append(new TextArgument($"-p:{keyValue.Key}={keyValue.Value}"));
            }

            return pag;
        };
    }

    private static IEnumerable<KeyValuePair<string, string>> GetKeyValuesFromString(string target, char delimiter = '=')
    {
        foreach (var property in GetFieldsFromLine(target))
        {
            var index = property.IndexOf(delimiter);
            if (index > -1)
            {
                yield return new KeyValuePair<string, string>(property.Substring(0, index), property.Substring(index + 1));
            }
        }
    }

    /// <summary>
    /// Get fields array from string using the specified delimiter.
    /// </summary>
    /// <param name="line">Target line to split.</param>
    /// <param name="delimiter">Delimiter.</param>
    /// <param name="quoteChar">Quote char.</param>
    /// <returns>Fields.</returns>
    /// <remarks>
    /// Source: https://www.codeproject.com/Tips/823670/Csharp-Light-and-Fast-CSV-Parser.
    /// </remarks>
    private static string[] GetFieldsFromLine(string line, char delimiter = ',', char quoteChar = '"')
    {
        var inQuote = false;
        var record = new List<string>();
        var sb = new StringBuilder();
        var reader = new StringReader(line);

        while (reader.Peek() != -1)
        {
            var readChar = (char)reader.Read();

            if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
            {
                // If it's a \r\n combo consume the \n part and throw it away.
                if (readChar == '\r')
                {
                    reader.Read();
                }

                if (inQuote)
                {
                    if (readChar == '\r')
                    {
                        sb.Append('\r');
                    }
                    sb.Append('\n');
                }
                else
                {
                    if (record.Count > 0 || sb.Length > 0)
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                }
            }
            else if (sb.Length == 0 && !inQuote)
            {
                if (readChar == quoteChar)
                {
                    inQuote = true;
                }
                else if (readChar == delimiter)
                {
                    record.Add(sb.ToString());
                    sb.Clear();
                }
                else if (char.IsWhiteSpace(readChar))
                {
                    // Ignore leading whitespace.
                }
                else
                {
                    sb.Append(readChar);
                }
            }
            else if (readChar == delimiter)
            {
                if (inQuote)
                {
                    sb.Append(delimiter);
                }
                else
                {
                    record.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else if (readChar == quoteChar)
            {
                if (inQuote)
                {
                    if ((char)reader.Peek() == quoteChar)
                    {
                        reader.Read();
                        sb.Append(quoteChar);
                    }
                    else
                    {
                        inQuote = false;
                    }
                }
                else
                {
                    sb.Append(readChar);
                }
            }
            else
            {
                sb.Append(readChar);
            }
        }

        if (record.Count > 0 || sb.Length > 0)
        {
            record.Add(sb.ToString());
        }

        return record.ToArray();
    }
}
