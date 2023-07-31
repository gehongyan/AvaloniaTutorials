

namespace ReactiveTests;

public class ObservableTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ObservableTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #region Generation

    /// <summary>
    ///     该示例演示了通过 Subject 管理 IObservable 的基本用法。
    ///     通过创建 Subject 作为 IObservable 对象，调用 OnNext、OnError 和 OnCompleted 方法来发出通知。
    ///     通过调用 Subscribe 方法来订阅 Subject 来接收通知。
    ///     OnNext 方法用于通知观察者，新的数据已经准备好了。
    ///     OnError 方法用于通知观察者，发生了错误。
    ///     OnCompleted 方法用于通知观察者，数据流已经结束。
    ///     该示例中通过 Subject 推送了 int 类型 0、1、150、15000 四个数据点，一个错误和一个完成通知。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.FromEventPattern 创建 IObservable 的基本用法。
    ///     通过 FromEventPattern 非泛型方法创建 IObservable 对象，
    ///     方法的第一个参数是用于将 EventHandler&lt;TEventArgs&gt;
    ///     委托转换为 NotifyCollectionChangedEventHandler 委托的类型转换。
    ///     在所订阅的事件发生时，会向 IObservable 推送新的数据点。
    ///     该示例中通过 FromEventPattern 方法推送了 NotifyCollectionChangedEventArgs。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.FromEventPattern 创建 IObservable 的基本用法。
    ///     通过 FromEventPattern 泛型方法创建 IObservable 对象，
    ///     在所订阅的事件发生时，会向 IObservable 推送新的数据点。
    ///     该示例中通过 FromEventPattern 方法推送了 RoutedEventArgs。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 IEnumerable&lt;T&gt;.ToObservable 创建 IObservable 的基本用法。
    ///     如需将非泛型 IEnumerable 转换为 IObservable，请先通过 Cast&lt;T&gt;
    ///     或 OfType&lt;T&gt; 方法将其转换为泛型 IEnumerable&lt;T&gt;。
    ///     该示例中通过 ToObservable 方法推送了 1 到 5 的整数序列。
    /// </summary>
    [Fact]
    public void Observable_IEnumerable()
    {
        IEnumerable<int> enumerableRange = Enumerable.Range(1, 5);
        IObservable<int> observableRange = enumerableRange.ToObservable();
        List<int> values = new();
        using IDisposable subscription = observableRange.Subscribe(x => values.Add(x));
        Assert.True(Enumerable.Range(1, 5).SequenceEqual(values));
    }

    /// <summary>
    ///     该示例演示了通过 Observable.Range 创建 IObservable 的基本用法。
    ///     Observable.Range 方法用于创建一个 IObservable 对象，该对象会向观察者推送指定范围内的整数序列。
    ///     该示例中通过 Range 方法推送了 1 到 5 的整数序列。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Where 限制 IObservable 的基本用法。
    ///     Observable.Where 方法用于创建一个 IObservable 对象，该对象会向观察者推送满足指定条件的数据点。
    ///     该示例中通过 Where 方法筛选出了小于 3 的数据点。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Where 限制 IObservable 的进阶用法。
    ///     示例中通过 Observable.FromEventPattern 创建的时间参数序列中包含了多种类型的事件，
    ///     通过 Where 方法筛选出了 NotifyCollectionChangedAction.Add 类型的事件。
    /// </summary>
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

    #region Aggregation

    /// <summary>
    ///     该示例演示了通过 Observable.Aggregate 静态聚合 IObservable 的基本用法。
    ///     Observable.Aggregate 方法用于创建一个 IObservable 对象，该对象会向观察者推送聚合后的数据点。
    ///     该示例中通过 Aggregate 方法推送了 1 到 5 的整数序列依次聚合的和。
    ///     Aggregate 方法要求 IObservable 序列已经完成，否则不会推送任何数据点。
    /// </summary>
    [Fact]
    public void Observable_Aggregate()
    {
        IObservable<int> range = Observable.Range(1, 5);
        int sum = 0;
        using IDisposable subscription = range.Aggregate((x, y) => x + y)
            .Subscribe(x =>
            {
                sum = x;
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        Assert.Equal(15, sum);
    }

    /// <summary>
    ///     该示例演示了通过 Observable.Scan 实时聚合 IObservable 的基本用法。
    ///     Observable.Scan 方法用于创建一个 IObservable 对象，该对象会向观察者推送聚合后的数据点。
    ///     该示例中通过 Scan 方法推送了 0 到 5 整数序列依次聚合的和。
    ///     Scan 方法不要求 IObservable 序列已经完成，可以在序列推送数据点的同时推送聚合后的数据点。
    /// </summary>
    [Fact]
    public async Task Observable_Scan()
    {
        IObservable<long> range = Observable.Interval(TimeSpan.FromSeconds(1));
        long sum = 0;
        using IDisposable subscription = range.Scan((x, y) => x + y)
            .Subscribe(x =>
            {
                sum = x;
                _testOutputHelper.WriteLine($"Number: {x}");
            });
        await Task.Delay(TimeSpan.FromSeconds(6.5));
        Assert.Equal(15, sum);
    }

    #endregion

    #region Time-based Generation

    /// <summary>
    ///     该示例演示了通过 Observable.Interval 定时创建 IObservable 的基本用法。
    ///     Observable.Interval 方法用于创建一个 IObservable 对象，该对象会向观察者按照指定的时间间隔推送自然数序列。
    ///     该示例中通过 Interval 方法推送了 0 到 4 的整数序列。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Timer 计时器创建 IObservable 的基本用法。
    ///     Observable.Timer 方法用于创建一个 IObservable 对象，该对象会向观察者在指定的初始化延迟后开始按照指定的时间间隔推送自然数序列。
    ///     Observable.Timer 的第一个参数为初始化延迟，第二个参数为时间间隔。
    ///     该示例中通过 Timer 方法推送了 0 到 3 的整数序列。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Generate 创建 IObservable 的基本用法。
    ///     Observable.Generate 方法用于创建一个 IObservable 对象，该对象会向观察者按照指定的时间间隔推送指定的序列。
    ///     方法的第一个参数为初始值，第二个参数为判断条件，第三个参数为迭代器，第四个参数为推送的数据点，第五个参数为时间间隔。
    ///     该示例中通过 Generate 方法推送了 0 到 9 的整数序列。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Delay 延迟 IObservable 的基本用法。
    ///     Observable.Delay 方法用于创建一个 IObservable 对象，该对象会向观察者推送延迟指定时间后的数据点。
    ///     该示例中通过 Delay 方法将原始序列的数据点推送延迟了 2 秒。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Throttle 节流 IObservable 的基本用法。
    ///     Observable.Throttle 方法用于创建一个 IObservable 对象，该对象会向观察者推送指定时间间隔内的最后一个数据点。
    ///     该示例中通过 Throttle 方法，将原始序列中与前一个数据点的时间间隔大于 500 毫秒的数据点推送给观察者。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Timeout 进行超时限制的 IObservable 的基本用法。
    ///     Observable.Timeout 方法用于创建一个 IObservable 对象，该对象会向观察者推送原始序列的数据点，
    ///     但是如果原始序列的数据点在指定的时间间隔内没有推送，则会向观察者推送 TimeoutException 异常。
    ///     该示例中通过 Timeout 方法，在原始序列超过 3 秒没有推送数据点时，会向观察者推送 TimeoutException 异常。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.TimeInterval 生成附带与前一数据点的时间间隔的 IObservable 的基本用法。
    ///     Observable.TimeInterval 方法用于创建一个 IObservable 对象，该对象会向观察者推送包含原始数据点及与前一个数据点的时间间隔的数据点。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Timestamp 生成附带时间戳的 IObservable 的基本用法。
    ///     Observable.Timestamp 方法用于创建一个 IObservable 对象，该对象会向观察者推送包含原始数据点及其时间戳的数据点。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Sample 采样 IObservable 的基本用法。
    ///     Observable.Sample 方法用于创建一个 IObservable 对象，该对象会向观察者推送原始序列中指定时间间隔内的最后一个数据点。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Buffer 缓冲切分 IObservable 的基本用法。
    ///     Observable.Buffer 方法用于创建一个 IObservable 对象，该对象会向观察者推送原始序列中按指定时间间隔分组的所有数据点。
    ///     该示例中通过 Buffer 方法，每经过 5 秒，会将刚过去 5 秒内的数据点作为一个 IList 推送给观察者。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Window 窗口化切分 IObservable 的基本用法。
    ///     Observable.Window 方法用于创建一个 IObservable 对象，该对象会向观察者推送原始序列中按指定时间间隔分组的子序列。
    ///     该示例中通过 Window 方法，每 5 秒创建一个子序列，并将接下来 5 秒内的数据点推送给该子序列。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Start 在后台线程执行代码的基本用法。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Start 在后台线程执行异步代码的基本用法。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.CombineLatest 合并多个 IObservable 任务进行并行执行的基本用法。
    ///     在 CombineLatest 方法中，每个 IObservable 任务都会在不同的线程中执行。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Merge 合并多个 IObservable 的基本用法。
    ///     在 Merge 中，每当任意一个 IObservable 推送新的数据点时，都只会将该数据点推送给观察者。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Publish 将非共享 IObservable 转换为共享 IConnectableObservable 的基本用法。
    ///     IObservable 可以被多个观察者订阅，每个观察者都会收到相同的数据序列。
    ///     非共享 IObservable 在订阅后是立即开始推送数据的，因此示例中的 #1 #2 对于整个序列的处理事件是先后发生的。
    ///     共享 IConnectableObservable 在订阅后需要调用 Connect 方法后才会开始推送数据，因此示例中的 #1 #2 的事件是以数据点为单位交替发生的。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Zip 按位次合并多个 IObservable 的基本用法。
    ///     在 Zip 中，每当所有 IObservable 都推送了新的数据点时，
    ///     都会将所有 IObservable 的最新数据点通过 resultSelector 函数合并为一个数据点推送给观察者。
    ///     实例中的 second 比 first 延迟了 500 毫秒，所以 first 一直都是在等待 second 推送数据后进行合并。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.CombineLatest 合并多个 IObservable 最新值的基本用法。
    ///     在 CombineLatest 中，每当任意一个 IObservable 推送新的数据点时，
    ///     都会将所有 IObservable 的最新数据点通过 resultSelector 函数合并为一个数据点推送给观察者。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Concat 冷连接多个 IObservable 的基本用法。
    ///     在 Concat 中，只有当前一个 IObservable 完成后，才会开始推送下一个 IObservable 的数据，
    ///     且被连接的 IObservable 会在连接后再开始推送数据。
    /// </summary>
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

    /// <summary>
    ///     该示例演示了通过 Observable.Concat 热连接多个 IObservable 的基本用法。
    ///     在 Concat 中，只有当前一个 IObservable 完成后，才会开始推送下一个 IObservable 的数据，
    ///     但实例对 second 调用了 Publish 及 Connect，使其在连接前就开始推送数据，但此时还未有任何订阅者。
    ///     当 first 完成后，连接的 second 所推送的数据点才开始被处理。
    /// </summary>
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
