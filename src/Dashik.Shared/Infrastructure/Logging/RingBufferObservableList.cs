using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Dashik.Shared.Infrastructure.Logging;

/// <summary>
/// This is a simple implementation of ring buffer list. I don't know the
/// best way to implement certain aspects, like insertion by index, size grow, etc.
/// </summary>
/// <typeparam name="T">List item type.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[Serializable]
public class RingBufferObservableList<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private const int DefaultCapacity = 100;

    private int _start;

    private int _version;

    private int _size;

    private T?[] _items;

    public RingBufferObservableList(int capacity = DefaultCapacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        _items = new T[capacity];
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => new RingBufferListEnumerator(this);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void Add(T item)
    {
        var currentPosition = TranslatePosition(_size);
        if (_size++ >= _items.Length)
        {
            _start++;
        }

        var oldestItem = _items[currentPosition];
        _items[currentPosition] = item;
        unchecked
        {
            _version++;
        }

        // Call events.
        if (_size - 1 >= _items.Length)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, oldestItem, 0));
        }
        else
        {
            OnCountPropertyChanged();
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, item, Count - 1));
        OnIndexerPropertyChanged();
    }

    /// <inheritdoc />
    public void Clear()
    {
        _size = 0;
        _start = 0;
        _version = 0;
        for (int i = 0; i < _items.Length; i++)
        {
            _items[i] = default;
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
    }

    /// <inheritdoc />
    public bool Contains(T item) => _size != 0 && IndexOf(item) != -1;

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        int i = 0;
        foreach (T item in this.Skip(arrayIndex))
        {
            array[i++] = item;
        }
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public int Count => _size >= _items.Length ? _items.Length : _size;

    /// <inheritdoc />
    public virtual bool IsReadOnly => false;

    /// <inheritdoc />
    public int IndexOf(T item)
    {
        var index = Array.IndexOf(_items, item, 0, _items.Length);
        if (index < 0)
        {
            return index;
        }
        return TranslatePosition(index + _start);
    }

    /// <inheritdoc />
    public void Insert(int index, T itemToInsert)
    {
        var newArr = new T[_items.Length];
        int i = 0, j = 0;
        foreach (T item in this)
        {
            if (j + 1> newArr.Length)
            {
                continue;
            }
            if (i++ == index)
            {
                newArr[j++] = itemToInsert;
                newArr[j++] = item;
                continue;
            }
            newArr[j++] = item;
        }

        _items = newArr;
        _start = 0;
        _version++;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, itemToInsert, index));
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        var originalItem = this[index];
        var newArr = new T[_items.Length];
        int i = 0, j = 0;
        foreach (T item in this)
        {
            if (i++ == index)
            {
                continue;
            }
            newArr[j++] = item;
        }

        _items = newArr;
        _start = 0;
        _size = Count - 1;
        _version++;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, originalItem, index));
        OnCountPropertyChanged();
        OnIndexerPropertyChanged();
    }

    /// <inheritdoc />
    public T this[int index]
    {
        get
        {
            var position = TranslatePosition(index + _start);
            return _items[position]!;
        }

        set
        {
            var position = TranslatePosition(index + _start);
            var originalValue = _items[position];
            _items[position] = value;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, originalValue, value, index));
            OnIndexerPropertyChanged();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int TranslatePosition(int position)
    {
        if (position < 0)
        {
            throw new IndexOutOfRangeException(nameof(position));
        }
        return position % _items.Length;
    }

    #region IEnumerator

    private struct RingBufferListEnumerator : IEnumerator<T>
    {
        private readonly RingBufferObservableList<T> _list;
        private int _index;
        private readonly int _version;
        private T? _current;

        internal RingBufferListEnumerator(RingBufferObservableList<T> list)
        {
            _list = list;
            _version = list._version;
            _current = default!;
            _index = 0;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            CheckVersion();
            if (_list.Count == _index)
            {
                return false;
            }
            var position = _list.TranslatePosition(_index++);
            _current = _list[position];
            return true;
        }

        /// <inheritdoc />
        public void Reset()
        {
            CheckVersion();
            _index = 0;
            _current = default!;
        }

        /// <inheritdoc />
        public T Current => _current!;

        /// <inheritdoc />
        object IEnumerator.Current => Current!;

        /// <inheritdoc />
        public void Dispose()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckVersion()
        {
            if (_version != _list._version)
            {
                throw new InvalidOperationException("The collection has changed");
            }
        }
    }

    #endregion

    #region INotifyCollectionChanged

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Helper to raise a PropertyChanged event for the Count property.
    /// </summary>
    private void OnCountPropertyChanged() => OnPropertyChanged(nameof(Count));

    /// <summary>
    /// Helper to raise a PropertyChanged event for the Indexer property.
    /// </summary>
    private void OnIndexerPropertyChanged() => OnPropertyChanged("Item[]");

    #endregion
}
