using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;

namespace SampleDataMaker.Domain.Services;

/// <summary>
/// 生成済みデータへ外部キー値とカテゴリレコード情報を反映します。
/// </summary>
public class ForeignKeyTestDataApplier : IForeignKeyTestDataApplier
{
    private readonly ISampleDataRepository _sampleDataRepository;

    public ForeignKeyTestDataApplier(ISampleDataRepository sampleDataRepository)
    {
        _sampleDataRepository = sampleDataRepository;
    }

    public IReadOnlyList<GeneratedTestData> Apply(
        IReadOnlyList<GeneratedTestData> testDataList,
        IReadOnlyList<ForeignKeyRelationSetting> settings)
    {
        if (settings.Count == 0)
        {
            return testDataList;
        }

        var generatedDataByTable = testDataList.ToDictionary(
            testData => CreateTableKey(testData.Table.SchemaName, testData.Table.TableName),
            MutableGeneratedTestData.Create);

        foreach (var setting in GetForwardSettings(settings))
        {
            ApplySetting(setting, generatedDataByTable);
        }

        return testDataList
            .Select(testData => generatedDataByTable[
                CreateTableKey(testData.Table.SchemaName, testData.Table.TableName)].ToGeneratedTestData())
            .ToList();
    }

    private void ApplySetting(
        ForeignKeyRelationSetting setting,
        IReadOnlyDictionary<string, MutableGeneratedTestData> generatedDataByTable)
    {
        var sourceTableKey = CreateTableKey(
            setting.SourceSchemaName,
            setting.SourceTableName);
        if (!generatedDataByTable.TryGetValue(sourceTableKey, out var sourceData))
        {
            return;
        }

        var referenceTableKey = CreateTableKey(
            setting.ReferenceSchemaName,
            setting.ReferenceTableName);
        generatedDataByTable.TryGetValue(referenceTableKey, out var referenceData);

        for (var rowIndex = 0; rowIndex < sourceData.Rows.Count; rowIndex++)
        {
            if (sourceData.RowMetadata[rowIndex].BoundaryValueColumns.Contains(setting.SourceColumnName))
            {
                continue;
            }

            if (referenceData == null || referenceData.Rows.Count == 0)
            {
                sourceData.Rows[rowIndex][setting.SourceColumnName] =
                    $"{setting.ReferenceTableName}_{setting.ReferenceColumnName}_{rowIndex + 1}";
                continue;
            }

            var referenceRowIndex = rowIndex % referenceData.Rows.Count;
            var referenceRow = referenceData.Rows[referenceRowIndex];

            if (referenceRow.TryGetValue(setting.ReferenceColumnName, out var referenceValue))
            {
                sourceData.Rows[rowIndex][setting.SourceColumnName] = referenceValue;
            }

            if (!referenceData.RowMetadata[referenceRowIndex].Columns.TryGetValue(
                setting.ReferenceColumnName,
                out var referenceMetadata))
            {
                sourceData.RowMetadata[rowIndex].Columns.Remove(setting.SourceColumnName);
                continue;
            }

            ApplyCategoryRecord(
                sourceData,
                rowIndex,
                setting.SourceColumnName,
                referenceMetadata);
        }
    }

    private void ApplyCategoryRecord(
        MutableGeneratedTestData sourceData,
        int rowIndex,
        string sourceColumnName,
        GeneratedColumnMetadata referenceMetadata)
    {
        var row = sourceData.Rows[rowIndex];
        var rowMetadata = sourceData.RowMetadata[rowIndex];
        var conflictingMetadata = rowMetadata.Columns.Values.FirstOrDefault(metadata =>
            metadata.CategoryName == referenceMetadata.CategoryName
            && metadata.IsForeignKeyInherited
            && metadata.CategoryRecordId != referenceMetadata.CategoryRecordId);

        if (conflictingMetadata != null)
        {
            throw new InvalidOperationException(
                $"同じ行のカテゴリ '{referenceMetadata.CategoryName}' に、"
                + $"異なる外部キーレコード '{conflictingMetadata.CategoryRecordId}' と"
                + $" '{referenceMetadata.CategoryRecordId}' が指定されています。");
        }

        if (!_sampleDataRepository.TryGetCategoryRecord(
            referenceMetadata.CategoryName,
            referenceMetadata.CategoryRecordId,
            out var record)
            || record == null)
        {
            throw new InvalidOperationException(
                $"カテゴリ '{referenceMetadata.CategoryName}' のレコード"
                + $" '{referenceMetadata.CategoryRecordId}' が見つかりません。");
        }

        var categoryColumns = rowMetadata.Columns
            .Where(pair => pair.Value.CategoryName == referenceMetadata.CategoryName)
            .Select(pair => pair.Key)
            .Append(sourceColumnName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var columnName in categoryColumns)
        {
            if (rowMetadata.BoundaryValueColumns.Contains(columnName))
            {
                continue;
            }

            var itemName = columnName == sourceColumnName
                ? referenceMetadata.CategoryItemName
                : rowMetadata.Columns[columnName].CategoryItemName;

            if (!record.Values.TryGetValue(itemName, out var value))
            {
                throw new InvalidOperationException(
                    $"カテゴリ '{referenceMetadata.CategoryName}' のレコード '{record.Id}' に"
                    + $"項目 '{itemName}' がありません。");
            }

            row[columnName] = value;
            rowMetadata.Columns[columnName] = new GeneratedColumnMetadata(
                columnName,
                referenceMetadata.CategoryName,
                itemName,
                record.Id,
                isForeignKeyInherited: true);
        }
    }

    private static IReadOnlyList<ForeignKeyRelationSetting> GetForwardSettings(
        IReadOnlyList<ForeignKeyRelationSetting> settings)
    {
        return settings
            .GroupBy(CreateUndirectedRelationKey)
            .Select(group => group.FirstOrDefault(setting => !setting.IsReverse) ?? group.First())
            .ToList();
    }

    private static string CreateUndirectedRelationKey(ForeignKeyRelationSetting setting)
    {
        var source = CreateColumnKey(
            setting.SourceSchemaName,
            setting.SourceTableName,
            setting.SourceColumnName);
        var reference = CreateColumnKey(
            setting.ReferenceSchemaName,
            setting.ReferenceTableName,
            setting.ReferenceColumnName);

        return string.CompareOrdinal(source, reference) <= 0
            ? $"{source}|{reference}"
            : $"{reference}|{source}";
    }

    private static string CreateColumnKey(string schemaName, string tableName, string columnName)
    {
        return $"{schemaName}.{tableName}.{columnName}";
    }

    private static string CreateTableKey(string schemaName, string tableName)
    {
        return $"{schemaName}.{tableName}";
    }

    /// <summary>
    /// 外部キー適用中に生成値とメタ情報を更新可能な形で保持します。
    /// </summary>
    private sealed class MutableGeneratedTestData
    {
        public DbTableInfo Table { get; }

        public IReadOnlyList<DbColumnInfo> Columns { get; }

        public List<Dictionary<string, string?>> Rows { get; }

        public List<MutableGeneratedRowMetadata> RowMetadata { get; }

        private MutableGeneratedTestData(
            DbTableInfo table,
            IReadOnlyList<DbColumnInfo> columns,
            List<Dictionary<string, string?>> rows,
            List<MutableGeneratedRowMetadata> rowMetadata)
        {
            Table = table;
            Columns = columns;
            Rows = rows;
            RowMetadata = rowMetadata;
        }

        public static MutableGeneratedTestData Create(GeneratedTestData source)
        {
            return new MutableGeneratedTestData(
                source.Table,
                source.Columns,
                source.Rows
                    .Select(row => new Dictionary<string, string?>(
                        row,
                        StringComparer.OrdinalIgnoreCase))
                    .ToList(),
                source.RowMetadata
                    .Select(MutableGeneratedRowMetadata.Create)
                    .ToList());
        }

        public GeneratedTestData ToGeneratedTestData()
        {
            return new GeneratedTestData(
                Table,
                Columns,
                Rows,
                RowMetadata.Select(metadata => metadata.ToGeneratedRowMetadata()).ToList());
        }
    }

    /// <summary>
    /// 外部キー適用中に1行分のメタ情報を更新可能な形で保持します。
    /// </summary>
    private sealed class MutableGeneratedRowMetadata
    {
        public int RowIndex { get; }

        public Dictionary<string, GeneratedColumnMetadata> Columns { get; }

        public IReadOnlySet<string> BoundaryValueColumns { get; }

        private MutableGeneratedRowMetadata(
            int rowIndex,
            Dictionary<string, GeneratedColumnMetadata> columns,
            IReadOnlySet<string> boundaryValueColumns)
        {
            RowIndex = rowIndex;
            Columns = columns;
            BoundaryValueColumns = boundaryValueColumns;
        }

        public static MutableGeneratedRowMetadata Create(GeneratedRowMetadata source)
        {
            return new MutableGeneratedRowMetadata(
                source.RowIndex,
                new Dictionary<string, GeneratedColumnMetadata>(
                    source.Columns,
                    StringComparer.OrdinalIgnoreCase),
                new HashSet<string>(
                    source.BoundaryValueColumns,
                    StringComparer.OrdinalIgnoreCase));
        }

        public GeneratedRowMetadata ToGeneratedRowMetadata()
        {
            return new GeneratedRowMetadata(RowIndex, Columns, BoundaryValueColumns);
        }
    }
}
