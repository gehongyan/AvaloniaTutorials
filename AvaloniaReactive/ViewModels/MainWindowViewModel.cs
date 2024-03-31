using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaReactive.Models;
using DynamicData;
using DynamicData.Binding;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaReactive.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Binding to IObservable

    /// <summary>
    ///     绑定到 IObservable，需要使用脱字符号（^）。这种方式不需要使用 [Reactive] 特性。
    /// </summary>
    public IObservable<long> SecondsFromStartup =>
        Observable.Interval(TimeSpan.FromSeconds(1))
            .StartWith(0);

    #endregion

    #region Binding via ObservableAsPropertyHelper

    /// <summary>
    ///     在 WPF 等不支持直接绑定到 IObservable 的框架中，可以使用 ObservableAsPropertyHelper 来实现绑定。
    /// </summary>
    private readonly ObservableAsPropertyHelper<long> _secondsFromHelper;

    public long SecondsFromHelper => _secondsFromHelper.Value;

    public MainWindowViewModel()
    {
        _secondsFromHelper =
            Observable.Interval(TimeSpan.FromSeconds(1))
                .StartWith(0)
                .ToProperty(this, x => x.SecondsFromHelper);
    }

    #endregion

    #region IObservable Basic

    /// <summary>
    ///     此处模拟一个搜索框，当输入内容时，会触发搜索，用于演示 IObservable 的一个实际的用法。
    /// </summary>
    [Reactive]
    public string? SearchQuery { get; set; }

    /// <summary>
    ///     这是一个将 NuGet 搜索结果进行扩展的类，用于在 UI 中显示。
    /// </summary>
    public record PackageSearchMetadataViewData(IPackageSearchMetadata Metadata)
    {
        /// <summary>
        ///     该命令包装了项目的 URL，用于在浏览器中打开。
        /// </summary>
        public ICommand OpenPage => ReactiveCommand.Create(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Metadata.ProjectUrl.ToString()
            });
        });

        /// <summary>
        ///     该任务包装了通过网络请求下载图标的任务，Avalonia 可以直接绑定到任务，使用脱字符号（^）。
        /// </summary>
        public Task<IImage> IconImage => Task.Run(async () =>
        {
            await Task.Yield();
            HttpClient httpClient = new();
            Stream dataStream = await httpClient.GetStreamAsync(Metadata.IconUrl);
            MemoryStream memStream = new();
            await dataStream.CopyToAsync(memStream);
            memStream.Position = 0;
            IImage newBitmap = new Bitmap(memStream);
            return newBitmap;
        });
    }

    /// <summary>
    ///     当搜索框中的内容发生变化时，会触发搜索，搜索结果会被绑定到这个属性上。
    /// </summary>
    public IObservable<IEnumerable<PackageSearchMetadataViewData>> SearchResults => this
        .WhenAnyValue(x => x.SearchQuery)
        .Throttle(TimeSpan.FromSeconds(1))
        .Select(term => term?.Trim())
        .DistinctUntilChanged()
        .Where(term => !string.IsNullOrWhiteSpace(term))
        .SelectMany(async x => await SearchNuGetPackagesAsync(x, CancellationToken.None))
        .Select(x => x.Take(4).Select(y => new PackageSearchMetadataViewData(y)));

    /// <summary>
    ///     搜索 NuGet 包。
    /// </summary>
    /// <param name="term">搜索关键字。</param>
    /// <param name="token">取消令牌。</param>
    /// <returns>搜索结果。</returns>
    private async Task<IEnumerable<IPackageSearchMetadata>> SearchNuGetPackagesAsync(string? term, CancellationToken token)
    {
        List<Lazy<INuGetResourceProvider>> providers = new();
        providers.AddRange(Repository.Provider.GetCoreV3());
        PackageSource package = new("https://api.nuget.org/v3/index.json");
        SourceRepository source = new(package, providers);

        SearchFilter filter = new(false);
        PackageSearchResource? resource = await source.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
        return await resource.SearchAsync(term, filter, 0, 10, new NuGet.Common.NullLogger(), token).ConfigureAwait(false);
    }

    #endregion

    #region WhenAnyValue

    /// <summary>
    ///     输入框的文字所绑定到的属性。虽然不会被用于更新视图，但需要用于通知其他属性该属性的变更。
    /// </summary>
    /// <remarks>
    ///     通过 IL Viewer，可以看到 [Reactive] 所生产的代码为：
    ///     <code language="csharp">
    ///         [Reactive]
    ///         public string? InputText
    ///         {
    ///           get =&gt; this.$InputText;
    ///           set =&gt; this.RaiseAndSetIfChanged&lt;MainWindowViewModel, string&gt;(ref this.$InputText, value, nameof (InputText));
    ///         }
    ///     </code>
    ///     <see cref="IReactiveObjectExtensions.RaiseAndSetIfChanged{TObj,TRet}"/>
    ///     方法内部调用了 <see cref="IReactiveObjectExtensions.RaisePropertyChanged{TSender}"/>，来通知其它属性该属性的变更。
    /// </remarks>
    [Reactive]
    public string? InputText { get; set; }

    /// <summary>
    ///     当输入框中的内容发生变化时，会触发计算输入框中的字符数。
    /// </summary>
    public IObservable<int> InputTextLength => this
        .WhenAnyValue(x => x.InputText)
        .Select(x => x?.Length ?? 0)
        .StartWith(InputText?.Length ?? 0);

    /// <summary>
    ///     当输入框中的内容发生变化时，会触发计算输入框中的数字数。
    /// </summary>
    public IObservable<int> DigitCount => this
        .WhenAnyValue(x => x.InputText)
        .Select(x => x?.Count(char.IsDigit) ?? 0)
        .StartWith(InputText?.Length ?? 0);

    /// <summary>
    ///     当输入框中的内容发生变化时，会触发判断输入框中的内容是否可以被转换为 <see cref="DateTime"/> 类型。
    /// </summary>
    public IObservable<bool> IsDateTime => this
        .WhenAnyValue(x => x.InputText)
        .Select(x => DateTime.TryParse(x, out _));

    /// <summary>
    ///     当输入框中的内容发生变化时，会触发判断输入框中的内容是否为手机号码。
    /// </summary>
    public IObservable<bool> IsPhoneNumber => this
        .WhenAnyValue(x => x.InputText)
        .Select(x => x?.Length == 11 && x.All(char.IsDigit));

    #endregion

    #region Command canExecute

    /// <summary>
    ///     模拟用户是否同意用户协议。
    /// </summary>
    [Reactive]
    public bool Agree { get; set; }

    /// <summary>
    ///     模拟用户名。
    /// </summary>
    [Reactive]
    public string? Username { get; set; }

    /// <summary>
    ///     模拟密码。
    /// </summary>
    [Reactive]
    public string? Password { get; set; }

    /// <summary>
    ///     通过一个简单的校验，为注册按钮的可用性提供支持。
    ///     当用户同意用户协议、用户名不为空、密码长度在 6 到 32 之间时，注册按钮可用。
    /// </summary>
    public ICommand RegisterCommand => ReactiveCommand.Create(() => { },
        canExecute: this.WhenAnyValue(
                x => x.Agree,
                x => x.Username,
                x => x.Password)
            .Select(x =>
                x.Item1
                && !string.IsNullOrWhiteSpace(x.Item2)
                && x.Item3 is { Length: >= 6 and <= 32 }));

    #endregion

    #region WhenAnyPropertyChanged

    /// <summary>
    ///     当 <see cref="Agree"/>、<see cref="Username"/>、<see cref="Password"/> 任意一个属性发生变化时，会触发计数。
    /// </summary>
    public IObservable<int> ChangedPropertyTriggeredCount => this.WhenAnyPropertyChanged(
            nameof(Agree),
            nameof(Username),
            nameof(Password))
        .Scan(0, (count, _) => count + 1)
        .StartWith(0);

    #endregion

    #region WhenAnyObservable

    /// <summary>
    ///     当 <see cref="SecondsFromStartup"/>、<see cref="InputTextLength"/>、<see cref="ChangedPropertyTriggeredCount"/>
    ///     任意一个属性发生变化时，会触发计算，生成信息概要。
    /// </summary>
    public IObservable<string> ObservableValuesSummary => this.WhenAnyObservable(
        x => x.SecondsFromStartup,
        x => x.InputTextLength,
        x => x.ChangedPropertyTriggeredCount,
        (seconds, length, count) => $"Current value: {seconds}s, {length} chars, {count} times changes");

    #endregion

    #region ObservableForProperty

    /// <summary>
    ///     当 <see cref="Password"/> 属性发生变化时，会触发计算，<c>beforeChange</c> 参数为 <c>true</c> 决定了变更触发前会被计算一次推送数据点。
    /// </summary>
    public IObservable<string> ObservableForPropertyBefore => this.ObservableForProperty(x => x.Password, true)
        .Select(x => x.Value ?? string.Empty)
        .StartWith(Password ?? string.Empty);

    /// <summary>
    ///     当 <see cref="Password"/> 属性发生变化时，会触发计算，<c>beforeChange</c> 参数为 <c>false</c> 决定了变更触发后会被计算一次推送数据点。
    /// </summary>
    public IObservable<string> ObservableForPropertyAfter => this.ObservableForProperty(x => x.Password, false)
        .Select(x => x.Value ?? string.Empty)
        .StartWith(Password ?? string.Empty);

    #endregion

    #region ObservableCollection

    /// <summary>
    ///     模拟一群学生。
    /// </summary>
    public ObservableCollection<Student> Students { get; } = new(Student.GetMany(5));

    /// <summary>
    ///     模拟添加学生。
    /// </summary>
    public ICommand AppendStudentCommand => ReactiveCommand.Create(() => Students.Add(Student.GetOne(Students.Count)));

    /// <summary>
    ///     模拟移除学生。
    /// </summary>
    public ICommand RemoveStudentCommand => ReactiveCommand.Create(() => Students.RemoveAt(Students.Count - 1));

    /// <summary>
    ///     计数学生数量，WhenAnyValue 是可以访问到属性的属性的。
    /// </summary>
    public IObservable<int> StudentsCount => this.WhenAnyValue(x => x.Students.Count);

    /// <summary>
    ///     计算学生中年龄大于 20 的学生数量，对 ObservableCollection 调用 ToObservableChangeSet，
    ///     可以将可观察集合转换为 IObservable&lt;IChangeSet&lt;T&gt;&gt;，
    ///     即转换为集合的变更信息。通过 Filter 进行过滤，而非 Where。
    ///     前者是对 IChangeSet 的泛型类型进行过滤，后者是对 IChangeSet 本身进行过滤。
    ///     调用 ToCollection 方法将 IObservable&lt;IChangeSet&lt;T&gt;&gt;
    ///     转换为 IObservable&lt;IReadOnlyCollection&lt;T&gt;&gt;，
    ///     即将集合的变更集转换为完整的集合。最后通过 Select 计算集合的数量。
    /// </summary>
    public IObservable<int> StudentsOlderThen20 => Students
        .ToObservableChangeSet()
        .Filter(x => x.Age > 20)
        .ToCollection()
        .Select(x => x.Count);

    #endregion

    #region Scheduler

    /// <summary>
    ///     在主线程上执行一个任务，在基于 XAML 的平台上类似 <c>Dispatcher.BeginInvoke</c>。
    /// </summary>
    public ICommand RxAppScheduleOnMainThreadCommand => ReactiveCommand.Create(() =>
        Observable.Start(() => Thread.Sleep(TimeSpan.FromSeconds(5)),
            RxApp.MainThreadScheduler));

    /// <summary>
    ///     在线程池上执行一个任务，类似 <c>Task.Run</c>。
    /// </summary>
    public ICommand RxAppScheduleOnTaskPoolCommand => ReactiveCommand.Create(() =>
        Observable.Start(() => Thread.Sleep(TimeSpan.FromSeconds(5)),
            RxApp.TaskpoolScheduler));

    #endregion
}
