using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Avalonia.Data;
using AvaloniaInputValidation.DataAnnotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaInputValidation.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyDataErrorInfo
{
    #region Casting Errors

    [Reactive]
    public string? CastingName { get; set; }

    [Reactive]
    public int CastingAge { get; set; }

    #endregion

    #region Setter Exceptions

    private string? _setterName;
    private int? _setterAge;

    public string? SetterName
    {
        get => _setterName;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(SetterName), "姓名不能为空。");
            if (value is not { Length: >= 6 and <= 10 })
                throw new ArgumentException("姓名长度必须大于等于 6 小于等于 10。", nameof(SetterName));
            this.RaiseAndSetIfChanged(ref _setterName, value);
        }
    }

    public int? SetterAge
    {
        get => _setterAge;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(SetterAge), "年龄不能为空。");
            if (value is not (> 0 and < 150))
                throw new ArgumentOutOfRangeException(nameof(SetterAge), "年龄必须大于 0 小于 150。");
            this.RaiseAndSetIfChanged(ref _setterAge, value);
        }
    }

    #endregion

    #region Throws DataValidationException

    private string? _dataValidationExceptionName;
    private int? _dataValidationExceptionAge;

    public string? DataValidationExceptionName
    {
        get => _dataValidationExceptionName;
        set
        {
            if (value is null)
                throw new DataValidationException("姓名不能为空。");
            if (value is not { Length: >= 6 and <= 10 })
                throw new DataValidationException("姓名长度必须大于等于 6 小于等于 10。");
            this.RaiseAndSetIfChanged(ref _dataValidationExceptionName, value);
        }
    }

    public int? DataValidationExceptionAge
    {
        get => _dataValidationExceptionAge;
        set
        {
            if (value is null)
                throw new DataValidationException("年龄不能为空。");
            if (value is not (> 0 and < 150))
                throw new DataValidationException("年龄必须大于 0 小于 150。");
            this.RaiseAndSetIfChanged(ref _dataValidationExceptionAge, value);
        }
    }

    #endregion

    #region DataAnnotations

    [Required]
    public string? AnnotationName { get; set; }

    [Range(0, 150)]
    public int? AnnotationAge { get; set; }

    [StringLength(32, MinimumLength = 6)]
    public string? AnnotationPassword { get; set; }

    [Compare(nameof(AnnotationPassword))]
    public string? AnnotationPasswordAgain { get; set; }

    [EmailAddress]
    public string? AnnotationEmail { get; set; }

    [Phone(ErrorMessage = "请输入正确的手机号码。")]
    public string? AnnotationPhone { get; set; }

    [FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "请选择正确的图片文件。")]
    public string? AnnotationFilename { get; set; }

    [RegularExpression(@"^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", ErrorMessage = "请输入正确的身份证号码。")]
    public string? AnnotationIdentityNumber { get; set; }

    [Guid]
    public string? AnnotationGuid { get; set; }

    #endregion

    #region INotifyDataErrorInfo

    private string? _notifyUsername;
    private string? _notifyPassword;
    private readonly Dictionary<string, List<string>> _errorsByPropertyName = new();

    public string? NotifyUsername
    {
        get => _notifyUsername;
        set
        {
            this.RaisePropertyChanging();
            ValidateUsername();
            this.RaiseAndSetIfChanged(ref _notifyUsername, value);
        }
    }

    public string? NotifyPassword
    {
        get => _notifyPassword;
        set
        {
            this.RaisePropertyChanging();
            ValidatePassword();
            this.RaiseAndSetIfChanged(ref _notifyPassword, value);
        }
    }

    public IEnumerable GetErrors(string? propertyName) =>
        propertyName is not null
        && _errorsByPropertyName.TryGetValue(propertyName, out List<string>? value)
            ? value
            : Enumerable.Empty<string>();

    /// <inheritdoc />
    public bool HasErrors => _errorsByPropertyName.Any();

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateUsername()
    {
        ClearErrors(nameof(NotifyUsername));
        if (string.IsNullOrWhiteSpace(NotifyUsername))
            AddError(nameof(NotifyUsername), "用户名不能为空。");
        if (string.Equals(NotifyUsername, "Admin", StringComparison.OrdinalIgnoreCase))
            AddError(nameof(NotifyUsername), "Admin 用户名已被占用。");
        if (NotifyUsername == null || NotifyUsername?.Length <= 5)
            AddError(nameof(NotifyUsername), "用户名长度至少为 6。");
    }

    private void ValidatePassword()
    {
        ClearErrors(nameof(NotifyPassword));
        if (string.IsNullOrWhiteSpace(NotifyPassword))
            AddError(nameof(NotifyPassword), "密码不能为空。");
        if (NotifyPassword == null || NotifyPassword?.Length <= 5)
            AddError(nameof(NotifyPassword), "密码长度至少为 6。");
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errorsByPropertyName.ContainsKey(propertyName))
            _errorsByPropertyName[propertyName] = new List<string>();

        if (!_errorsByPropertyName[propertyName].Contains(error))
        {
            _errorsByPropertyName[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    private void ClearErrors(string propertyName) =>
        _errorsByPropertyName.Remove(propertyName);

    #endregion

    #region Custom DataValidationErrors ControlTheme

    [Required(ErrorMessage = "用户名不能为空。")]
    public string? CustomErrorControlThemeUsername { get; set; }

    #endregion
}
