using System.Collections.Generic;

public class HistoryQueue<T>
{
    public int Count => queue.Count;

    private int capacity;
    private Queue<T> queue;

    private T[] array = null;

    public HistoryQueue(int capacity)
    {
        this.capacity = capacity;
        queue = new Queue<T>();
    }

    public void Enqueue(T item)
    {
        while (queue.Count >= capacity)
        {
            queue.Dequeue();
        }

        queue.Enqueue(item);
        array = queue.ToArray();
    }

    public void Clear()
    {
        queue.Clear();
        array = queue.ToArray();
    }

    public T At(int i)
    {
        return array[i];
    }
}