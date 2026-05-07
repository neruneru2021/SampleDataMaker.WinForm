using SampleDataMaker.Domain.Entities;

namespace SampleDataMaker.Domain.Services;

public class ForeignKeyTestDataApplier : IForeignKeyTestDataApplier
{
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
            testData => testData);
        var result = new List<GeneratedTestData>();

        foreach (var testData in testDataList)
        {
            var tableSettings = settings
                .Where(setting => IsSourceTable(setting, testData.Table))
                .ToList();

            if (tableSettings.Count == 0)
            {
                result.Add(testData);
                continue;
            }

            var rows = new List<IReadOnlyDictionary<string, string?>>();

            for (var rowIndex = 0; rowIndex < testData.Rows.Count; rowIndex++)
            {
                var row = new Dictionary<string, string?>(testData.Rows[rowIndex]);

                foreach (var setting in tableSettings)
                {
                    row[setting.SourceColumnName] = CreateReferenceValue(
                        setting,
                        generatedDataByTable,
                        rowIndex);
                }

                rows.Add(row);
            }

            result.Add(new GeneratedTestData(testData.Table, testData.Columns, rows));
        }

        return result;
    }

    private static string? CreateReferenceValue(
        ForeignKeyRelationSetting setting,
        IReadOnlyDictionary<string, GeneratedTestData> generatedDataByTable,
        int rowIndex)
    {
        var referenceTableKey = CreateTableKey(
            setting.ReferenceSchemaName,
            setting.ReferenceTableName);

        if (generatedDataByTable.TryGetValue(referenceTableKey, out var referenceData)
            && referenceData.Rows.Count > 0)
        {
            var referenceRow = referenceData.Rows[rowIndex % referenceData.Rows.Count];

            if (referenceRow.TryGetValue(setting.ReferenceColumnName, out var value))
            {
                return value;
            }
        }

        return $"{setting.ReferenceTableName}_{setting.ReferenceColumnName}_{rowIndex + 1}";
    }

    private static bool IsSourceTable(
        ForeignKeyRelationSetting setting,
        DbTableInfo table)
    {
        return setting.SourceSchemaName == table.SchemaName
            && setting.SourceTableName == table.TableName;
    }

    private static string CreateTableKey(string schemaName, string tableName)
    {
        return $"{schemaName}.{tableName}";
    }
}
