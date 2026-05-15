using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Dashik.Sdk.Models;

namespace Dashik.Widgets.Motd;

public partial class MotdSettingsSectionControl : UserControl
{
    public MotdWidgetSettings Settings => (MotdWidgetSettings)((SettingsSectionModel)DataContext!).Settings!;

    public MotdSettingsSectionControl()
    {
        InitializeComponent();
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender == null)
        {
            return;
        }
        var msg = (Motd)((Button)sender).Tag!;
        Settings.Messages.Remove(msg);
    }

    private void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Settings.Messages.Add(new Motd());
    }

    private async void LoadCsvButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load messages from CSV",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("CSV")
                {
                    Patterns = ["*.csv"],
                    MimeTypes = ["text/csv"],
                },
                FilePickerFileTypes.All,
            ],
        });

        if (files.Count == 0)
        {
            return;
        }

        await using var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream);

        var isFirstRow = true;
        foreach (var row in ParseCsv(reader))
        {
            if (isFirstRow)
            {
                isFirstRow = false;
                continue;
            }

            if (row.Count == 0)
            {
                continue;
            }

            var text = row[0].Trim();
            if (text.Length == 0)
            {
                continue;
            }

            Settings.Messages.Add(new Motd(text));
        }
    }

    private static IEnumerable<IReadOnlyList<string>> ParseCsv(TextReader reader)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var rowHasContent = false;

        while (true)
        {
            var read = reader.Read();
            if (read == -1)
            {
                if (rowHasContent || field.Length > 0 || fields.Count > 0)
                {
                    fields.Add(field.ToString());
                    yield return fields;
                }
                yield break;
            }

            var c = (char)read;

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (reader.Peek() == '"')
                    {
                        reader.Read();
                        field.Append('"');
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                    rowHasContent = true;
                }
                else if (c == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    rowHasContent = true;
                }
                else if (c == '\r')
                {
                    if (reader.Peek() == '\n')
                    {
                        reader.Read();
                    }
                    fields.Add(field.ToString());
                    field.Clear();
                    var row = fields;
                    fields = new List<string>();
                    var hadContent = rowHasContent;
                    rowHasContent = false;
                    if (hadContent || row.Count > 1 || (row.Count == 1 && row[0].Length > 0))
                    {
                        yield return row;
                    }
                }
                else if (c == '\n')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    var row = fields;
                    fields = new List<string>();
                    var hadContent = rowHasContent;
                    rowHasContent = false;
                    if (hadContent || row.Count > 1 || (row.Count == 1 && row[0].Length > 0))
                    {
                        yield return row;
                    }
                }
                else
                {
                    field.Append(c);
                    rowHasContent = true;
                }
            }
        }
    }
}
