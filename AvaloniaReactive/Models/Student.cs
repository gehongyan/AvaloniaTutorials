using System.Collections.Generic;

namespace AvaloniaReactive.Models;

public record Student(string Name, int Age, string Id)
{
    public static Student GetOne(int order) =>
        new($"Student {order}", order + 18, $"{order + 1000}");

    public static IEnumerable<Student> GetMany(int count, int start = 0)
    {
        for (int i = 0; i < count; i++)
            yield return new Student($"Student {i + start}", i + start + 18, $"{i + start + 1000}");
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name}, {Age} year{(Age == 1 ? string.Empty : "s")}, ID: {Id}";
}
