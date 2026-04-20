using Xunit;
using Dashik.Abstractions;
using Dashik.Shared.Services.Packages;

namespace Dashik.Tests.Services;

/// <summary>
/// Tests for <see cref="WidgetPackageGroup" />.
/// </summary>
public class WidgetPackageGroupTests
{
    private static LocalWidgetPackage CreateLocal(string id, string version)
        => new($"/tmp/{id}.nupkg")
        {
            Id = id,
            Name = id + " Local",
            Version = version,
        };

    private static RemoteWidgetPackage CreateRemote(string id, string version, string storageUri)
        => new(storageUri, new WidgetPackage
        {
            Id = id,
            Name = id + " Remote",
            Version = version,
        });

    [Fact]
    public void Combine_OnlyLocalPackage_ReturnsGroupWithLocal()
    {
        var local = new[]
        {
            CreateLocal("pkg1", "1.0.0")
        };
        var result = WidgetPackageGroup.Combine([], local, []);

        Assert.Single(result);
        Assert.Equal("pkg1", result[0].Local!.Id);
        Assert.Null(result[0].Remote);
        Assert.True(result[0].Installed);
        Assert.False(result[0].UpToDate);
        Assert.False(result[0].HasUpdate);
    }

    [Fact]
    public void Combine_OnlyRemotePackage_ReturnsGroupWithRemote()
    {
        var remote = new[]
        {
            CreateRemote("pkg2", "2.0.0", "http://feed/")
        };
        var result = WidgetPackageGroup.Combine([], [], remote);

        Assert.Single(result);
        Assert.Equal("pkg2", result[0].Remote!.Id);
        Assert.Null(result[0].Local);
        Assert.False(result[0].Installed);
    }

    [Fact]
    public void Combine_LocalAndRemotePackage_UpToDateAndHasUpdate()
    {
        var local = new[]
        {
            CreateLocal("pkg3", "2.0.0")
        };
        var remote = new[]
        {
            CreateRemote("pkg3", "2.0.0", "http://feed/")
        };
        var result = WidgetPackageGroup.Combine([], local, remote);

        Assert.Single(result);
        Assert.True(result[0].UpToDate);
        Assert.False(result[0].HasUpdate);
        // Now test update available.
        remote = [CreateRemote("pkg3", "3.0.0", "http://feed/")];
        result = WidgetPackageGroup.Combine([], local, remote);
        Assert.Single(result);
        Assert.False(result[0].UpToDate);
        Assert.True(result[0].HasUpdate);
    }
}
