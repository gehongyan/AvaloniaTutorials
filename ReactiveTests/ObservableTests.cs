using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Xunit.Abstractions;

namespace ReactiveTests;

public class ObservableTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ObservableTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #region Generation

    [Fact]
    public void Observable_Basic()
    {
        Subject<int> subject = new();
        IObservable<int> observable = subject;
        int value = 0;
        using IDisposable subscription = observable.Subscribe(x => value = x);
        Assert.Equal(0, value);

        subject.OnNext(1);
        Assert.Equal(1, value);

        subject.OnNext(150);
        Assert.Equal(150, value);

        Assert.Throws<InvalidOperationException>(() =>
            subject.OnError(new InvalidOperationException("Test")));

        subject.OnCompleted();
        subject.OnNext(15000);
        Assert.Equal(150, value);
    }

    [Fact]
    public void Observable_FromEventPattern()
    {
        int raisedCount = 0;
        ObservableCollection<int> collection = new();
        IObservable<EventPattern<NotifyCollectionChangedEventArgs>> eventObservable = Observable
            .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                ev => new NotifyCollectionChangedEventHandler(ev),
                ev => collection.CollectionChanged += ev,
                ev => collection.CollectionChanged -= ev);
        using IDisposable subscription = eventObservable.Subscribe(_ => raisedCount++);
        Assert.Equal(0, raisedCount);
        collection.Add(1);
        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void Observable_FromEventPatternGeneric()
    {
        int raisedCount = 0;
        Button button = new();
        IObservable<EventPattern<RoutedEventArgs>> eventObservable = Observable
            .FromEventPattern<RoutedEventArgs>(
                x => button.Click += x,
                x => button.Click -= x);
        using IDisposable subscription = eventObservable.Subscribe(_ => raisedCount++);
        Assert.Equal(0, raisedCount);
        button.RaiseEvent(new RoutedEventArgs { RoutedEvent = Button.ClickEvent });
        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void Observable_IEnumerable()
    {
        IEnumerable<int> enumerableRange = Enumerable.Range(1, 5);
        IObservable<int> observableRange = enumerableRange.ToObservable();
        List<int> values = new();
        using IDisposable subscription = observableRange.Subscribe(x => values.Add(x));
        Assert.True(Enumerable.Range(1, 5).SequenceEqual(values));
    }

    [Fact]
    public void Observable_Range()
    {
        IObservable<int> range = Observable.Range(1, 5);
        List<int> values = new();
        using IDisposable subscription = range.Subscribe(x => values.Add(x));
        Assert.True(Enumerable.Range(1, 5).SequenceEqual(values));
    }

    #endregion

    #region Restriction

    [Fact]
    public async Task Observable_Where_1()
    {
        IObservable<long> oneNumberPerSecond = Observable.Interval(TimeSpan.FromSeconds(1))
            .Where(x => x < 3);
        List<long> values = new();
        using IDisposable subscription = oneNumberPerSecond.Subscribe(x =>
        {
            values.Add(x * 2);
            _testOutputHelper.WriteLine($"Number: {x}");
        });
        await Task.Delay(TimeSpan.FromSeconds(5.5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public void Observable_Where_2()
    {
        int raisedCount = 0;
        ObservableCollection<int> collection = new();
        IObservable<EventPattern<NotifyCollectionChangedEventArgs>> eventObservable = Observable
            .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                ev => new NotifyCollectionChangedEventHandler(ev),
                ev => collection.CollectionChanged += ev,
                ev => collection.CollectionChanged -= ev)
            .Where(x => x.EventArgs.Action == NotifyCollectionChangedAction.Add);
        using IDisposable subscription = eventObservable.Subscribe(_ => raisedCount++);
        Assert.Equal(0, raisedCount);
        collection.Add(1);
        collection.Remove(1);
        Assert.Equal(1, raisedCount);
    }

    #endregion

    #region Time-based Generation

    [Fact]
    public async Task Observable_Interval()
    {
        IObservable<long> oneNumberPerSecond = Observable.Interval(TimeSpan.FromSeconds(1));
        List<long> values = new();
        using IDisposable subscription = oneNumberPerSecond
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Timer()
    {
        IObservable<long> observable = Observable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
        List<long> values = new();
        using IDisposable subscription = observable
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Generate()
    {
        IObservable<int> oneNumberPerSecond = Observable.Generate(0,
            x => x < 10,
            x => x + 1,
            x => x,
            x => TimeSpan.FromSeconds(1));
        List<long> values = new();
        using IDisposable subscription = oneNumberPerSecond
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        await oneNumberPerSecond;
        Assert.NotEmpty(values);
    }

    #endregion

    #region Time-based Restriction

    [Fact]
    public async Task Observable_Delay()
    {
        IObservable<long> observable = Observable.Interval(TimeSpan.FromMilliseconds(500));
        List<long> values = new();
        using IDisposable subscription = observable
            .Delay(TimeSpan.FromSeconds(2))
            .Buffer(TimeSpan.FromSeconds(1))
            .Subscribe(x =>
            {
                values.AddRange(x);
                _testOutputHelper.WriteLine($"Number: {string.Join(",", x)}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Throttle()
    {
        IObservable<int> observable = Observable.Generate(0,
            x => x < 30,
            x => x + 1,
            x => x,
            x => x % 10 is < 3 or > 8 ? TimeSpan.FromSeconds(1) : TimeSpan.FromMilliseconds(200));
        List<int> values = new();
        using IDisposable subscription = observable
            .Delay(TimeSpan.FromMilliseconds(200))
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {string.Join(",", x)}");
            });
        await Task.Delay(TimeSpan.FromSeconds(15));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Timeout()
    {
        IObservable<Timestamped<long>> observable = Observable.Timer(TimeSpan.FromSeconds(5)).Timestamp();
        IObservable<Timestamped<long>> observableWithTimeoutShort = observable.Timeout(TimeSpan.FromSeconds(3));

        using IDisposable disposableShort = observableWithTimeoutShort.Subscribe(
            x => _testOutputHelper.WriteLine($"{x.Value}: {x.Timestamp}"),
            ex => _testOutputHelper.WriteLine($"{ex.Message} {DateTime.Now}"));
        await Assert.ThrowsAsync<TimeoutException>(async () => await observableWithTimeoutShort);

        IObservable<Timestamped<long>> observableWithTimeoutLong = observable.Timeout(TimeSpan.FromSeconds(6));
        using IDisposable disposableLong = observableWithTimeoutLong.Subscribe(
            x => _testOutputHelper.WriteLine($"{x.Value}: {x.Timestamp}"),
            ex => _testOutputHelper.WriteLine($"{ex.Message} {DateTime.Now}"));
        Exception? exception = await Record.ExceptionAsync(async () => await observableWithTimeoutLong);
        Assert.Null(exception);
    }

    #endregion

    #region Time-based Projection

    [Fact]
    public async Task Observable_TimeInterval()
    {
        IObservable<TimeInterval<long>> intervals = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .TimeInterval();
        List<long> values = new();
        using IDisposable subscription = intervals
            .Subscribe(x =>
            {
                values.Add(x.Value);
                _testOutputHelper.WriteLine($"Number: {x.Value}, Interval: {x.Interval}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Timestamp()
    {
        IObservable<Timestamped<long>> intervals = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Timestamp();
        List<long> values = new();
        using IDisposable subscription = intervals
            .Subscribe(x =>
            {
                values.Add(x.Value);
                _testOutputHelper.WriteLine($"Number: {x.Value}, Interval: {x.Timestamp}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Sample()
    {
        IObservable<long> observable = Observable.Interval(TimeSpan.FromMilliseconds(20));
        List<long> values = new();
        using IDisposable subscription = observable
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {string.Join(",", x)}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Buffer()
    {
        IObservable<long> observable = Observable.Interval(TimeSpan.FromMilliseconds(500));
        List<long> values = new();
        using IDisposable subscription = observable
            .Buffer(TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                values.AddRange(x);
                _testOutputHelper.WriteLine($"Number: {string.Join(",", x)}");
            });
        await Task.Delay(TimeSpan.FromSeconds(10));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Window()
    {
        IObservable<long> mainSequence = Observable.Interval(TimeSpan.FromMilliseconds(500));
        List<long> values = new();
        mainSequence
            .Window(() => Observable.Interval(TimeSpan.FromSeconds(5)))
            .Subscribe(x =>
            {
                _testOutputHelper.WriteLine($"New window: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
                x.Subscribe(y =>
                {
                    values.Add(y);
                    _testOutputHelper.WriteLine($"Number: {y}");
                });
            });
        await Task.Delay(TimeSpan.FromSeconds(10));
        Assert.NotEmpty(values);
    }

    #endregion

    #region Background Processing

    [Fact]
    public async Task Observable_RunCode()
    {
        bool completed = false;
        IObservable<Unit> observable = Observable.Start(() =>
        {
            _testOutputHelper.WriteLine("From background thread. Does not block main thread.");
            _testOutputHelper.WriteLine("Calculating...");
            Task.Delay(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();
            completed = true;
            _testOutputHelper.WriteLine("Background work completed.");
        }).Finally(() => _testOutputHelper.WriteLine("Main thread completed."));
        _testOutputHelper.WriteLine("In Main Thread...");
        Assert.False(completed);
        await observable;
        Assert.True(completed);
    }

    [Fact]
    public async Task Observable_RunCodeAsynchronously()
    {
        bool completed = false;
        IObservable<Task> observable = Observable.Start(async () =>
        {
            _testOutputHelper.WriteLine("From background thread. Does not block main thread.");
            _testOutputHelper.WriteLine("Calculating...");
            await Task.Delay(TimeSpan.FromSeconds(3));
            completed = true;
            _testOutputHelper.WriteLine("Background work completed.");
        }).Finally(() => _testOutputHelper.WriteLine("Main thread completed."));
        _testOutputHelper.WriteLine("In Main Thread...");
        Assert.False(completed);
        await await observable;
        Assert.True(completed);
    }

    [Fact]
    public async Task Observable_ParallelExecution()
    {
        IObservable<IList<string>> observable = Observable.CombineLatest(
            Observable.Start(() =>
            {
                _testOutputHelper.WriteLine($"Executing 1st on Thread: {Environment.CurrentManagedThreadId}");
                return "Result A";
            }),
            Observable.Start(() =>
            {
                _testOutputHelper.WriteLine($"Executing 2nd on Thread: {Environment.CurrentManagedThreadId}");
                return "Result B";
            }),
            Observable.Start(() =>
            {
                _testOutputHelper.WriteLine($"Executing 3rd on Thread: {Environment.CurrentManagedThreadId}");
                return "Result C";
            })
        ).Finally(() => _testOutputHelper.WriteLine("Done!"));

        IList<string> results = await observable.FirstAsync();
        Assert.NotEmpty(results);
        foreach (string result in results)
            _testOutputHelper.WriteLine(result);
    }

    #endregion

    #region Combinition

    [Fact]
    public async Task Observable_Merge()
    {
        IObservable<string> first = Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(x => $"#1: {x}");
        IObservable<string> second = Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(x => $"#2: {x}")
            .Delay(TimeSpan.FromMilliseconds(500));
        List<string> values = new();
        using IDisposable subscription = first
            .Merge(second)
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Publish()
    {
        List<string> values = new();
        IObservable<int> unshared = Observable.Range(1, 5);
        _testOutputHelper.WriteLine("Subscribing to unshared");
        unshared.Select(x => $"#1: {x}").Subscribe(x =>
        {
            values.Add(x);
            _testOutputHelper.WriteLine($"Unshared {x}");
        });
        unshared.Select(x => $"#2: {x}").Subscribe(x =>
        {
            values.Add(x);
            _testOutputHelper.WriteLine($"Unshared {x}");
        });
        _testOutputHelper.WriteLine("Subscribed to unshared");
        await unshared;
        Assert.NotEmpty(values);
        values.Clear();

        IConnectableObservable<int> shared = unshared.Publish();
        _testOutputHelper.WriteLine("Subscribing to shared");
        shared.Select(x => $"#1: {x}").Subscribe(x =>
        {
            values.Add(x);
            _testOutputHelper.WriteLine($"Shared {x}");
        });
        shared.Select(x => $"#2: {x}").Subscribe(x =>
        {
            values.Add(x);
            _testOutputHelper.WriteLine($"Shared {x}");
        });
        _testOutputHelper.WriteLine("Subscribed to shared");
        _testOutputHelper.WriteLine("Connecting");
        using IDisposable disposable = shared.Connect();
        _testOutputHelper.WriteLine("Connected");
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Zip()
    {
        IObservable<string> first = Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(x => $"#1: {x}");
        IObservable<string> second = Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(x => $"#2: {x}")
            .Delay(TimeSpan.FromMilliseconds(500));
        List<string> values = new();
        using IDisposable subscription = first
            .Zip(second, (x, y) => $"{x} & {y}")
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_CombineLatest()
    {
        IObservable<string> first = Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(x => $"#1: {x}");
        IObservable<string> second = Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(x => $"#2: {x}")
            .Delay(TimeSpan.FromMilliseconds(500));
        List<string> values = new();
        using IDisposable subscription = first
            .CombineLatest(second, (x, y) => $"{x} & {y}")
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Concat_Cold()
    {
        IObservable<string> first = Observable.Generate(0,
                x => x < 5,
                x => x + 1,
                x => x,
                x => TimeSpan.FromMilliseconds(500))
            .Select(x => $"#1: {x}");
        IObservable<string> second = Observable.Generate(0,
                x => x < 10,
                x => x + 1,
                x => x,
                x => TimeSpan.FromMilliseconds(500))
            .Select(x => $"#2: {x}");
        List<string> values = new();
        using IDisposable subscription = first
            .Concat(second)
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            }, () => _testOutputHelper.WriteLine("Completed"));
        await Task.Delay(TimeSpan.FromSeconds(10));
        Assert.NotEmpty(values);
    }

    [Fact]
    public async Task Observable_Concat_Hot()
    {
        IObservable<string> first = Observable.Generate(0,
                x => x < 5,
                x => x + 1,
                x => x,
                x => TimeSpan.FromMilliseconds(500))
            .Select(x => $"#1: {x}");
        IConnectableObservable<string> second = Observable.Generate(0,
                x => x < 10,
                x => x + 1,
                x => x,
                x => TimeSpan.FromMilliseconds(500))
            .Select(x => $"#2: {x}").Publish();
        using IDisposable disposable = second.Connect();
        List<string> values = new();
        using IDisposable subscription = first
            .Concat(second)
            .Subscribe(x =>
            {
                values.Add(x);
                _testOutputHelper.WriteLine($"Number: {x}");
            }, () => _testOutputHelper.WriteLine("Completed"));
        await Task.Delay(TimeSpan.FromSeconds(10));
        Assert.NotEmpty(values);
    }

    #endregion

}
