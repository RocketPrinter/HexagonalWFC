using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RandomQueue<T> : IEnumerable<T>
{
    readonly SortedDictionary<double, T> dict = new();
    readonly System.Random rand;

    public RandomQueue() => rand = new();

    public RandomQueue(System.Random rand) => this.rand = rand;

    public int Count => dict.Count;
    public IEnumerable<T> Values => dict.Values;

    // higher weight means more likely to be chosen
    public void Push(T item, double weight = 1)
    {
        while (!dict.TryAdd(rand.NextDouble() / weight, item)) { }
    }

    public T PopRandom()
    {
        (double key, T value) = dict.First();
        dict.Remove(key);
        return value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Values).GetEnumerator();
    }
}
