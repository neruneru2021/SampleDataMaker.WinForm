namespace SampleDataMaker.Domain.Services;

/// <summary>
/// カラムの「種類」コンボボックスで使う予約済みの生成モード名です。
/// </summary>
public static class SampleDataKindNames
{
    public const string Normal = "Normal";

    public const string Random = "Random";

    public static bool IsNormal(string? sampleDataKind)
    {
        return string.IsNullOrWhiteSpace(sampleDataKind)
            || sampleDataKind.Equals(Normal, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsRandom(string? sampleDataKind)
    {
        return !string.IsNullOrWhiteSpace(sampleDataKind)
            && sampleDataKind.Equals(Random, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsReserved(string? sampleDataKind)
    {
        return IsNormal(sampleDataKind) || IsRandom(sampleDataKind);
    }
}
