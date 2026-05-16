using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

/// <summary>
/// サンプルデータ設定テンプレートをコンボボックスに表示するためのアイテムです。
/// </summary>
internal class ColumnSampleDataTemplateSelectionItem
{
    public string DisplayName { get; }

    public ColumnSampleDataTemplate Template { get; }

    /// <summary>
    /// テンプレート情報から表示名を組み立てます。
    /// </summary>
    public ColumnSampleDataTemplateSelectionItem(ColumnSampleDataTemplate template)
    {
        Template = template;
        DisplayName = $"{template.SchemaName}.{template.TableName} {template.TemplateName}".Trim();
    }

    /// <summary>
    /// コンボボックス表示用の名称を返します。
    /// </summary>
    public override string ToString()
    {
        return DisplayName;
    }
}
