using System.Collections.Generic;
using System.Linq;

public class RandomQueue<T>
{
    readonly SortedDictionary<int, T> dict = new();
    readonly System.Random rand;

    public RandomQueue() => rand = new();

    public RandomQueue(int seed) => rand = new(seed);

    public int Count => dict.Count;
    public IEnumerable<T> Values => dict.Values;

    public void Push(T item)
    {
        while (!dict.TryAdd(rand.Next(), item)) { }
    }

    public T PopRandom()
    {
        (int key, T value) = dict.First();
        dict.Remove(key);
        return value;
    }

}
