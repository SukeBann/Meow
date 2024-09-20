using System.Collections.ObjectModel;

/// <summary>
/// 表示一个具有固定容量的可观察集合。
/// 当集合达到容量时，新添加的元素将会移除最早进入的元素。
/// </summary>
/// <typeparam name="T">集合中元素的类型。</typeparam>
public class FixedCapacityObservableCollection<T> : ObservableCollection<T>
{
    private readonly int _capacity;

    /// <summary>
    /// 使用指定的容量初始化 <see cref="FixedCapacityObservableCollection{T}"/> 类的新实例。
    /// </summary>
    /// <param name="capacity">集合的固定容量。</param>
    public FixedCapacityObservableCollection(int capacity)
    {
        _capacity = capacity;
    }

    /// <summary>
    /// 将元素插入集合中的指定位置。
    /// 如果集合已达到最大容量，将移除最早进入的元素。
    /// </summary>
    /// <param name="index">插入元素的位置。</param>
    /// <param name="item">要插入的元素。</param>
    protected override void InsertItem(int index, T item)
    {
        if (Count >= _capacity)
        {
            RemoveAt(0);
        }
        base.InsertItem(index, item);
    }
}