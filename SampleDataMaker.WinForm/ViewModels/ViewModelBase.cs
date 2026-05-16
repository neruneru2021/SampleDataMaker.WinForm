using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// ViewModel共通のプロパティ変更通知とUIスレッドへの通知戻しを提供します。
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// UIスレッド上でPropertyChangedを発行するための同期コンテキストです。
    /// </summary>
    private readonly SynchronizationContext _syncContext;

    /// <summary>
    /// ViewModel生成時のUI同期コンテキストを保持します。
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
    /// バインド先へプロパティ変更を通知します。
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// テスト時に差し替え可能な現在日時を返します。
    /// </summary>
    public virtual DateTime GetDateTime()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// 値が変わった場合だけフィールドを更新し、UIスレッド上で変更通知を発行します。
    /// </summary>
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
