using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.WinForm.ViewModels;

internal class ColumnSampleDataTemplateSelectionItem
{
    public string DisplayName { get; }

    public ColumnSampleDataTemplate Template { get; }

    public ColumnSampleDataTemplateSelectionItem(ColumnSampleDataTemplate template)
    {
        Template = template;
        DisplayName = $"{template.SchemaName}.{template.TableName} {template.TemplateName}".Trim();
    }

    public override string ToString()
    {
        return DisplayName;
    }
}
