using Xunit;
using System.Collections.Specialized;
using Dashik.Host.Utils;

namespace Dashik.Tests.Utils;

/// <summary>
/// Tests for <see cref="CollectionUtils" />.
/// </summary>
public class CollectionUtilsTests
{
    [Fact]
    public void SyncFromChangedEventArg_Add_InsertsItemsAtSpecifiedIndex()
    {
        // Arrange.
        var dest = new List<int> { 1, 4 };
        var ev = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            new List<int> { 2, 3 },
            1);

        // Act.
        CollectionUtils.SyncFromChangedEventArg(ev, dest);

        // Assert.
        Assert.Equal([1, 2, 3, 4], dest);
    }

    [Fact]
    public void SyncFromChangedEventArg_Remove_RemovesItemsByIndexRange()
    {
        // Arrange.
        var dest = new List<int> { 1, 2, 3, 4, 5 };
        var ev = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove,
            new List<int> { 2, 3 },
            1);

        // Act.
        CollectionUtils.SyncFromChangedEventArg(ev, dest);

        // Assert.
        Assert.Equal([1, 4, 5], dest);
    }

    [Fact]
    public void SyncFromChangedEventArg_Replace_ReplacesItemsAtIndex()
    {
        // Arrange.
        var dest = new List<int> { 1, 2, 3, 4 };
        var ev = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace,
            new List<int> { 7, 8 },
            new List<int> { 2, 3 },
            1);

        // Act.
        CollectionUtils.SyncFromChangedEventArg(ev, dest);

        // Assert.
        Assert.Equal([1, 7, 8, 4], dest);
    }

    [Fact]
    public void SyncFromChangedEventArg_MoveSingle_MovesItemToNewIndex()
    {
        // Arrange.
        var dest = new List<int> { 1, 2, 3, 4, 5 };
        var ev = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Move,
            new List<int> { 2 },
            3,
            1);

        // Act.
        CollectionUtils.SyncFromChangedEventArg(ev, dest);

        // Assert.
        Assert.Equal([1, 3, 4, 2, 5], dest);
    }

    [Fact]
    public void SyncFromChangedEventArg_MoveMultiple_MovesItemBlockWithoutChangingOrder()
    {
        // Arrange.
        var dest = new List<int> { 1, 2, 3, 4, 5 };
        var ev = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Move,
            new List<int> { 2, 3 },
            3,
            1);

        // Act.
        CollectionUtils.SyncFromChangedEventArg(ev, dest);

        // Assert.
        Assert.Equal([1, 4, 5, 2, 3], dest);
    }

    [Fact]
    public void SyncFromChangedEventArg_Reset_ClearsDestination()
    {
        // Arrange.
        var dest = new List<int> { 1, 2, 3 };
        var ev = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        // Act.
        CollectionUtils.SyncFromChangedEventArg(ev, dest);

        // Assert.
        Assert.Empty(dest);
    }
}
