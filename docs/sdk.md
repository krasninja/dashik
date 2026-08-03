# Dashik SDK

The Dashik SDK is the public surface used to build **widgets** for the Dashik dashboard application. A widget is a self-contained Avalonia control plus metadata, optional settings, and optional lifecycle hooks. The host application discovers widgets by reflecting over assemblies and looking for types that:

1. Implement [`IWidget`](#iwidget),
2. Carry a [`[WidgetInfo]`](#widgetinfoattribute) attribute.

The host then creates the widget, calls `InitializeAsync`, mounts its `Control` in the dashboard, and - depending on which optional interfaces the widget implements - drives settings UI, periodic updates, previews, menus, state persistence, etc.

- **Target framework:** `net10.0`
- **UI framework:** Avalonia 12 + ReactiveUI.Avalonia
- **Package id:** `Dashik.Sdk`
- **License:** see project repository
- **Project URL:** https://github.com/krasninja/dashik

## Installation

```bash
dotnet add package Dashik.Sdk
```

Your widget project should target `net10.0` and reference the SDK. A typical widget assembly is a class library that produces a single `.dll` or `.nupkg` consumed by the host as a package.

## Quick Start: Minimum Viable Widget

```csharp
using Avalonia.Controls;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;

namespace MyCompany.Widgets.Hello;

[WidgetInfo(
    id: "com.mycompany.widgets.hello",
    name: "Hello",
    Description = "Says hello.",
    Category = WidgetCategory.Misc)]
public sealed class HelloWidget : IWidget
{
    public string Header => "Hello";

    public Control Control { get; } = new TextBlock { Text = "Hello, Dashik!" };

    public Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

That is the full contract: an attribute, a header, a control, an initialization method. Everything else is optional and additive - implement one of the `IWidget*` interfaces below to opt in to a capability.

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│ Host application (Dashik)                                    │
│  - Loads widget assemblies                                   │
│  - Discovers types via [WidgetInfo] + IWidget                │
│  - Constructs widget instances (DI: IWidgetContext)          │
│  - Calls InitializeAsync(WidgetInitInfo)                     │
│  - Mounts Control in the dashboard                           │
│  - Calls IWidgetUpdate.UpdateAsync on a timer                │
│  - Drives settings UI via IWidgetSettings                    │
│  - Persists state via IWidgetState                           │
└──────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────┐
│ Widget                                                       │
│  [WidgetInfo(id, name, ...)]                                 │
│  class MyWidget : IWidget                                    │
│                    , IWidgetSettings   // optional           │
│                    , IWidgetUpdate     // optional           │
│                    , IWidgetPreview    // optional           │
│                    , IWidgetMenu       // optional           │
│                    , IWidgetTrayMenu   // optional           │
│                    , IWidgetBadges     // optional           │
│                    , IWidgetText       // optional           │
│                    , IWidgetState      // optional           │
│                    , IWidgetDataList   // optional           │
└──────────────────────────────────────────────────────────────┘
```

The widget instance and its `Control` are long-lived; the host calls `UpdateAsync` repeatedly on the configured interval. Settings are JSON-backed and re-applied across runs.

## Widget Metadata

### `WidgetInfoAttribute`

Required on every widget class. Used by the host to discover and describe the widget.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class WidgetInfoAttribute : Attribute
{
    public WidgetInfoAttribute(string id, string name);

    public string Id { get; }                     // e.g. "com.acme.widgets.clock"
    public string Name { get; }                   // user-visible name
    public string Description { get; set; }       // optional
    public Type? SettingsType { get; set; }       // optional, your settings class
    public Type? InfoType { get; set; }           // optional, custom WidgetInfo subclass
    public WidgetCategory Category { get; set; }  // default Misc
}
```

**Conventions:**
- `Id` should be a reverse-DNS-style globally unique string. It is the stable identifier across versions.
- `Name` is for UI; keep short.
- `SettingsType` is the type the host will use to deserialize persisted user settings. Combine with `IWidgetSettings` to expose a settings UI.

### `WidgetCategory`

Enum used to group widgets in the "Add Widget" picker. Values include `Misc` (default), `Accessibility`, `ApplicationLaunchers`, `Clipboard`, `DateTime`, `EnvironmentWeather`, `FileSystem`, `FunGames`, `Graphics`, `Language`, `Multimedia`, `OnlineServices`, `SystemInformation`, `Productivity`, `SoftwareDevelopment`, `Utilities`.

### `WidgetInfo` (class)

Runtime descriptor built by the host from the attribute. Exposes `Id`, `Name`, `Description`, `WidgetType`, `SettingsType`, `Icon`, `DefaultUpdateInterval`. You can subclass it and set `InfoType` on the attribute to enrich the description (e.g., provide a custom icon or a longer default update interval).

```csharp
public class WidgetInfo
{
    public string Id { get; }
    public Type WidgetType { get; }
    public Type? SettingsType { get; }
    public string Name { get; }
    public string Description { get; }
    public WidgetInfoAttribute Info { get; }
    public IImage Icon { get; protected set; }                      // default = generic icon
    public TimeSpan DefaultUpdateInterval { get; protected set; }   // default = 5 min
}
```

---

## Core Interface: `IWidget`

Every widget must implement this.

```csharp
public interface IWidget
{
    string Header { get; }                                  // title shown in the widget chrome
    Control Control { get; }                                // the Avalonia control to render
    Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken cancellationToken = default);
}
```

`InitializeAsync` is called once after construction. Use it to apply settings, load remote data, wire up timers, etc. Long initialization should respect the cancellation token.

### `WidgetInitInfo`

Bundles everything the host hands the widget at startup.

```csharp
public sealed class WidgetInitInfo
{
    public IWidgetContext Context { get; }            // host-provided runtime services
    public WidgetMainSettings MainSettings { get; }   // common settings (update interval, proxy, etc.)
    public JsonObject Settings { get; }               // raw widget-specific JSON

    public T GetSettings<T>() where T : class;        // deserialize into your settings type
    public object GetSettings(Type type);
}
```

`GetSettings<T>()` returns a new `T` if deserialization fails — never throws on malformed JSON. Use it as the canonical way to materialize your typed settings inside `InitializeAsync`.

## Host-Provided Services

### `IWidgetContext`

Available via `WidgetInitInfo.Context`. Provides primitives that respect host configuration (notably the user-configured web proxy).

```csharp
public interface IWidgetContext
{
    HttpClient CreateHttpClient();   // honors WidgetMainSettings.WebProxy
    bool PreviewMode { get; }        // true when the widget runs inside the preview window
}
```

Always use `Context.CreateHttpClient()` instead of `new HttpClient()` so user proxy settings are applied.

`PreviewMode` lets the widget skip side effects (network calls, persistence) when running in the "Add Widget" preview.

### `WidgetMainSettings`

Common per-instance settings the host owns. Read these - don't write - except for `CustomTitle`, `Hidden`, etc., which the host UI controls.

| Member            | Type        | Notes |
|-------------------|-------------|-------|
| `UpdateInterval`  | `TimeSpan`  | How often `IWidgetUpdate.UpdateAsync` fires. Default 5 min. |
| `CustomTitle`     | `string`    | Override for the widget header. |
| `UseCustomTitle`  | `bool`      | Whether the custom title is applied. |
| `Height`          | `double`    | Target widget height. |
| `Disabled`        | `bool`      | When true, host suppresses updates. |
| `Hidden`          | `bool`      | Content visually hidden. |
| `SpaceId`         | `string`    | Which dashboard "space" the widget lives in. |
| `WebProxy`        | `string`    | HTTP/SOCKS proxy URL (e.g., `socks5://127.0.0.1:1080`). Applied by `CreateHttpClient()`. |

---

## Capability Interfaces

Implement any subset on the same class as `IWidget`. The host probes for each at runtime.

### `IWidgetUpdate`

Periodic refresh hook.

```csharp
public interface IWidgetUpdate
{
    Task UpdateAsync(CancellationToken cancellationToken = default);
}
```

Called on the cadence set by `WidgetMainSettings.UpdateInterval`. Skipped when `Disabled` is true. Throw `WidgetException` to surface a user-visible error in the widget; throw `WidgetNotConfiguredException` to ask the host to render the "Configure" prompt instead of an error.

### `IWidgetSettings`

Expose a settings panel inside the Settings dialog.

```csharp
public interface IWidgetSettings
{
    object Settings { get; }
    Type SettingsType { get; }
    IReadOnlyList<SettingsSection> GetSections();
}
```

`GetSections()` returns one or more sections. Each section is built with `SettingsSection.Create<TControl, TViewModel>(name)` and binds:
- `TControl`: an Avalonia `Control` rendered in the section pane,
- `TViewModel`: a class inheriting `SettingsSectionModel` whose `Settings` property is set by the host to the widget's typed settings object.

```csharp
public sealed class SettingsSection
{
    public string Name { get; }
    public IImage? Icon { get; set; }
    public Type ControlType { get; }
    public Type ViewModelType { get; }

    public static SettingsSection Create<TControl, TViewModel>(string name)
        where TControl : Control where TViewModel : SettingsSectionModel;
    public static SettingsSection Create<TControl>(string name)
        where TControl : Control;
}

public class SettingsSectionModel : ReactiveObject
{
    public object? Settings { get; set; }              // host injects your typed settings
    public virtual void SyncSetting();                 // called when Settings is (re)assigned
    public virtual Task LoadAsync(CancellationToken cancellationToken = default);
}
```

Override `SyncSetting()` to react to settings being applied; override `LoadAsync` for async loading when the section opens.

### `IWidgetPreview`

Powers the "Add Widget" preview gallery.

```csharp
public interface IWidgetPreview
{
    IReadOnlyList<WidgetPreview> GetPreviewConfigurations();
    void SetPreview(WidgetPreview widgetPreview);
}

public class WidgetPreview
{
    public string Name { get; }
    public string Description { get; init; }
    public object? Settings { get; init; }   // typically your settings type, host will apply
    public object? Data { get; }             // free-form payload for the widget to read
    public WidgetPreview(string name, object? data = null);
}
```

Return one or more named preview configurations. The host will call `SetPreview` with the chosen one when constructing the widget in preview mode. While previewing, `IWidgetContext.PreviewMode` is `true`.

### `IWidgetMenu`

Add items to the widget's per-instance context menu.

```csharp
public interface IWidgetMenu
{
    IReadOnlyList<MenuItem> GetWidgetMenuItems();
}
```

Returns Avalonia `MenuItem` instances. Wire up `Click` handlers yourself.

### `IWidgetTrayMenu`

Add items to the OS system tray menu.

```csharp
public interface IWidgetTrayMenu
{
    ObservableCollection<NativeMenuItem> TrayMenuItems { get; }
}
```

The collection is observable — mutate it at any time to add/remove tray entries.

### `IWidgetBadges`

Show small labeled counters in the widget header.

```csharp
public interface IWidgetBadges
{
    ObservableCollection<WidgetBadge> Badges { get; }
}

public class WidgetBadge : ReactiveObject
{
    public string Name { get; set; }
    public int Value { get; set; }
    public Color Color { get; set; }   // default DarkGray
    public WidgetBadge();
    public WidgetBadge(string name, int value);
}
```

### `IWidgetText`

Expose the widget's content as plain text - used for compact rendering modes, copying, tray summaries, etc.

```csharp
public interface IWidgetText
{
    string GetText(WidgetTextMode mode);
}

public enum WidgetTextMode
{
    Compact,    // one-line summary
    Expand,     // multi-line full content
}
```

### `IWidgetState`

Persist runtime state that is *not* user settings (e.g., scroll position, cached values).

```csharp
public interface IWidgetState
{
    Task SetStateAsync(object state, CancellationToken cancellationToken = default);
    Task<object> GetStateAsync(Type stateType, CancellationToken cancellationToken = default);
}
```

The state object must be JSON-serializable.

### `IWidgetDataList`

Expose underlying data for host-driven filtering/searching.

```csharp
public interface IWidgetDataList
{
    IReadOnlyList<object> DataList { get; set; }
}
```

## Exceptions

The SDK exception hierarchy is used by the host to render appropriate UI:

```
Exception
└── DashikException                      // generic SDK-level error
    └── WidgetException                  // user-visible error within a widget
        └── WidgetNotConfiguredException // host renders "Configure" affordance
```

Throw `WidgetNotConfiguredException` from `InitializeAsync` or `UpdateAsync` when the widget cannot proceed without user input (e.g., missing API key). The host will show a "Configure" button that opens the settings dialog.

Throw `WidgetException` for recoverable runtime errors that should be shown verbatim to the user. Anything else is treated as an unexpected fault.

## Application Services

### `IMvvmService`

Injected by the host. Use it to open windows, look up views, or construct view models via the host's container.

```csharp
public interface IMvvmService
{
    Control? FindControlByViewModel(object viewModel);
    Window? GetMainWindow();
    object CreateViewModel(Type type, params object[] parameters);

    Task OpenAsync(object viewModel, CancellationToken cancellationToken = default);
    Task<DialogResult> OpenAsync<TDialogResult>(
        IDialogViewModel<TDialogResult> viewModel,
        CancellationToken cancellationToken = default);
}
```

### Dialog Contracts

```csharp
public interface ICloseableViewModel
{
    event EventHandler? CloseRequest;
}

public interface IDialogViewModel<out TDialogResult>
{
    TDialogResult ResultValue { get; }
    DialogResult Result { get; }
}

public enum DialogResult
{
    Cancel, OK, Abort, Retry, Ignore, Yes, No, TryAgain, Continue
}
```

A dialog view model raises `CloseRequest` to ask the window to close, exposes `Result` (the button pressed) and `ResultValue` (the typed payload).

### Built-in Dialogs

- **`MessageBoxViewModel`** (`Dashik.Sdk.ViewModels`) — a fully featured message box with configurable buttons and icon. Use the `SetOkMode()`, `SetOkCancelMode()`, `SetErrorMode()`, `SetYesNoMode()`, `SetYesNoCancelMode()`, `SetAbortRetryIgnoreMode()` helpers, then open via `IMvvmService.OpenAsync`. Returns a `DialogResult`.

  ```csharp
  var vm = new MessageBoxViewModel("Delete this item?", "Confirm").SetYesNoMode();
  var result = await mvvmService.OpenAsync(vm);
  if (result == DialogResult.Yes) { /* ... */ }
  ```

- **`TextWindowViewModel`** (`Dashik.Sdk.ViewModels`) — a simple text-display window with a `Close` command. Useful for showing logs, long errors, etc.

### `UiContextUtils`

```csharp
public static class UiContextUtils
{
    public static SwitchToUiAwaitable SwitchToUi();       // await this to resume on UI thread
    public static void Invoke(Action callback);           // invoke now or marshal to UI
}
```

Idiomatic usage:

```csharp
// Doing background work, then touching UI:
var data = await FetchAsync(ct);
await UiContextUtils.SwitchToUi();
ViewModel.Items = data;
```

`Invoke` is short-circuited when the caller is already on the UI thread (no marshalling cost).

### `ImageUtils`

Helpers for loading Avalonia `Bitmap` instances.

```csharp
public static class ImageUtils
{
    public static Bitmap LoadFromResource(Uri resourceUri);          // avares:// URIs
    public static Task<Bitmap?> LoadFromWeb(string? url);            // returns null on failure
    public static Task<Bitmap?> LoadFromWeb(Uri url);
}
```

Note: `LoadFromWeb` uses a private static `HttpClient` and does **not** apply the user's configured web proxy. For proxy-aware fetching, build your own request with `IWidgetContext.CreateHttpClient()`.

### `EmbeddedResourceUtils`

```csharp
public static class EmbeddedResourceUtils
{
    public static Bitmap GetAsBitmap(string uri, Assembly? assembly = null);  // cached
    public static string GetAsText(string uri, Assembly? assembly = null);
}
```

Resolves manifest resources relative to the calling assembly by default. Bitmaps are cached process-wide.

### `IdGenerator`

```csharp
public static string Generate(string? prefix = null, int length = 12);
```

Produces an upper-alphanumeric random string. Useful for stable instance IDs inside settings.

## Application (`Dashik.Sdk.Application`)

Static utilities for asking the host about its environment.

```csharp
public static class Application
{
    public const string ProductName = "Dashik";

    public static string GetVersion();                   // e.g. "0.7.0-alpha.16+abcdef..."
    public static string GetShortVersion();              // e.g. "0.7.0-alpha.16"
    public static string GetProductFullName();           // "Dashik <version>"

    public static string GetPlatform();                  // PlatformLinux | Windows | MacOS | ...
    public static string GetArchitecture();              // ArchitectureX64 | Arm64 | ...
    public static string GetRuntimeIdentifier();         // e.g. "linux-x64"
}
```

Constants `PlatformLinux`, `PlatformWindows`, `PlatformMacOS`, `PlatformFreeBSD`, `PlatformAndroid`, `PlatformIOS`, `PlatformBrowser`, `PlatformUnknown` and `ArchitectureX86`/`X64`/`Arm`/`Arm64`/`Wasm`/`Msil`/`Unknown` make string-matching robust.

## Worked Example: MOTD Widget

The reference widget that ships with Dashik is a "message of the day" picker. It illustrates the common pattern of implementing several capability interfaces on a single class:

```csharp
[WidgetInfo(
    id: "com.antisoft.widgets.motd",
    name: "Motd",
    Description = "Displays the message of the day.",
    Category = WidgetCategory.Misc,
    InfoType = typeof(MotdWidgetInfo))]
public sealed class MotdWidget : IWidget, IWidgetSettings, IWidgetPreview, IWidgetUpdate, IWidgetMenu
{
    private readonly IWidgetContext _context;

    public string Header => "MOTD";
    public Control Control { get; }
    public object Settings { get; set; } = new MotdWidgetSettings();
    public Type SettingsType => typeof(MotdWidgetSettings);

    public MotdWidget(IWidgetContext context)
    {
        _context = context;
        Control = new MotdWidgetControl { DataContext = new MotdWidgetViewModel() };
    }

    public Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken ct = default)
    {
        Settings = initInfo.GetSettings<MotdWidgetSettings>();
        ApplyMotd();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CancellationToken ct = default) => Task.CompletedTask;

    public IReadOnlyList<SettingsSection> GetSections() =>
    [
        SettingsSection.Create<MotdSettingsSectionControl>("MOTD"),
    ];

    public IReadOnlyList<WidgetPreview> GetPreviewConfigurations() =>
    [
        new WidgetPreview("MOTD message")
        {
            Settings = new MotdWidgetSettings { Messages = [new Motd("Welcome to Dashik!")] }
        },
    ];

    public void SetPreview(WidgetPreview preview) { /* apply preview.Settings */ }

    public IReadOnlyList<MenuItem> GetWidgetMenuItems()
    {
        var mi = new MenuItem { Header = "New MOTD" };
        mi.Click += (_, _) => ApplyMotd();
        return [mi];
    }
}
```

Things to notice:

- The constructor takes `IWidgetContext` — the host's DI container injects it.
- `Settings` is replaced inside `InitializeAsync` using `WidgetInitInfo.GetSettings<T>()`.
- `GetSections` returns a single section bound to a custom Avalonia control. The control's view model inherits `SettingsSectionModel` and reads/writes the typed settings.
- `GetPreviewConfigurations` returns curated demo data, so the preview gallery is non-empty.

---

## Conventions and Recommendations

- **One concrete widget class per `[WidgetInfo]`.** Discovery is class-level.
- **Don't block in `InitializeAsync`.** It runs on the UI thread; use `await` and respect the cancellation token.
- **Surface configuration problems with `WidgetNotConfiguredException`** so the host renders the proper UX instead of a stack trace.
- **Keep settings types plain.** They are serialized to JSON; use simple properties with parameterless constructors.
- **Use `UiContextUtils.SwitchToUi()`** when transitioning from background work to UI mutation rather than `Dispatcher.UIThread.Post` directly.
- **Stable IDs.** `WidgetInfoAttribute.Id` is the identity used to migrate persisted settings — never change it after release.

## Reference Index

| Symbol | Kind | Namespace |
|---|---|---|
| `IWidget` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetBadges` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetContext` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetDataList` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetMenu` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetPreview` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetSettings` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetState` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetText` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetTrayMenu` | interface | `Dashik.Sdk.Abstract` |
| `IWidgetUpdate` | interface | `Dashik.Sdk.Abstract` |
| `WidgetTextMode` | enum | `Dashik.Sdk.Abstract` |
| `WidgetInfoAttribute` | attribute | `Dashik.Sdk.Widgets` |
| `WidgetInfo` | class | `Dashik.Sdk.Widgets` |
| `WidgetInitInfo` | class | `Dashik.Sdk.Widgets` |
| `WidgetCategory` | enum | `Dashik.Sdk.Widgets` |
| `WidgetMainSettings` | class | `Dashik.Sdk.Models` |
| `WidgetBadge` | class | `Dashik.Sdk.Models` |
| `WidgetPreview` | class | `Dashik.Sdk.Models` |
| `WidgetPackage` | class | `Dashik.Sdk.Models` |
| `SettingsSection` | class | `Dashik.Sdk.Models` |
| `SettingsSectionModel` | class | `Dashik.Sdk.Models` |
| `IMvvmService` | interface | `Dashik.Sdk.Mvvm` |
| `MvvmServiceExtensions` | static class | `Dashik.Sdk.Mvvm` |
| `IDialogViewModel<T>` | interface | `Dashik.Sdk.Mvvm` |
| `ICloseableViewModel` | interface | `Dashik.Sdk.Mvvm` |
| `DialogResult` | enum | `Dashik.Sdk.Mvvm` |
| `MessageBoxViewModel` | class | `Dashik.Sdk.ViewModels` |
| `TextWindowViewModel` | class | `Dashik.Sdk.ViewModels` |
| `MessageBoxWindow` | window | `Dashik.Sdk.Views` |
| `TextWindow` | window | `Dashik.Sdk.Views` |
| `Application` | static class | `Dashik.Sdk` |
| `DashikException` | exception | `Dashik.Sdk` |
| `WidgetException` | exception | `Dashik.Sdk` |
| `WidgetNotConfiguredException` | exception | `Dashik.Sdk` |
| `UiContextUtils` | static class | `Dashik.Sdk.Utils` |
| `ImageUtils` | static class | `Dashik.Sdk.Utils` |
| `EmbeddedResourceUtils` | static class | `Dashik.Sdk.Utils` |
| `IdGenerator` | static class | `Dashik.Sdk.Utils` |
| `ResourceDescriptionAttribute` | attribute | `Dashik.Sdk.Utils` |
