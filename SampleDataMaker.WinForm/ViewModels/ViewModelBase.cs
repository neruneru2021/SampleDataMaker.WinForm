using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// ViewModel基底クラス
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// 通知用
    /// </summary>
    private readonly SynchronizationContext _syncContext;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    protected ViewModelBase()
    {
        // UI コンテキストがあるタイミングで作り、
        // ViewModelBase 側では new SynchronizationContext() に逃がさないことです。
        _syncContext = SynchronizationContext.Current
            ?? throw new InvalidOperationException(
                "ViewModelはUIスレッド上で生成してください。");
    }

    /// <summary>
    /// 通知用
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Mock側でオーバーライドする為、virtualは必須
    /// </summary>
    /// <returns>DateTime</returns>
    public virtual DateTime GetDateTime()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// プロパティセット用共通処理
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="field">field</param>
    /// <param name="value">value</param>
    /// <param name="propertyName">propertyName</param>
    /// <returns>更新有無</returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;

        if (PropertyChanged == null)
        {
            return true;
        }

        if (SynchronizationContext.Current == _syncContext)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        else
        {
            _syncContext.Post(_ =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }, null);
        }

        return true;
    }
}
