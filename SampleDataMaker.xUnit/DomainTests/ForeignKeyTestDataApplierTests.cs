using ChainingAssertion;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Services;

namespace SampleDataMaker.xUnit.DomainTests;

/// <summary>
/// 生成済みテストデータへ外部キー値を反映する処理を確認します。
///
/// 外部キー適用はDBへ問い合わせず、生成済みデータ同士を見比べて値を差し替えます。
/// そのため、GeneratedTestDataを直接組み立てるだけで単体テストできます。
/// </summary>
[TestClass]
public sealed class ForeignKeyTestDataApplierTests
{
    [TestMethod]
    public void 参照先テーブルに生成済み行がある場合は参照先カラムの値をコピーする()
    {
        // Arrange
        var applier = new ForeignKeyTestDataApplier();
        var parentData = CreateGeneratedData(
            tableName: "Clinics",
            rows: new[]
            {
                Row(("ClinicId", "C001")),
                Row(("ClinicId", "C002"))
            });
        var childData = CreateGeneratedData(
            tableName: "Users",
            rows: new[]
            {
                Row(("UserId", "U001"), ("ClinicId", "before-1")),
                Row(("UserId", "U002"), ("ClinicId", "before-2")),
                Row(("UserId", "U003"), ("ClinicId", "before-3"))
            });
        var settings = new[]
        {
            new ForeignKeyRelationSetting
            {
                SourceSchemaName = "dbo",
                SourceTableName = "Users",
                SourceColumnName = "ClinicId",
                ReferenceSchemaName = "dbo",
                ReferenceTableName = "Clinics",
                ReferenceColumnName = "ClinicId"
            }
        };

        // Act
        var result = applier.Apply(new[] { parentData, childData }, settings);
        var appliedChildData = result.Single(x => x.Table.TableName == "Users");

        // Assert
        // 参照先が2行、参照元が3行なので、3行目は1行目の参照先値へ循環します。
        appliedChildData.Rows[0]["ClinicId"].Is("C001");
        appliedChildData.Rows[1]["ClinicId"].Is("C002");
        appliedChildData.Rows[2]["ClinicId"].Is("C001");
    }

    [TestMethod]
    public void 参照先テーブルがない場合は分かりやすい代替値を作成する()
    {
        // Arrange
        var applier = new ForeignKeyTestDataApplier();
        var childData = CreateGeneratedData(
            tableName: "Users",
            rows: new[]
            {
                Row(("UserId", "U001"), ("ClinicId", "before-1"))
            });
        var settings = new[]
        {
            new ForeignKeyRelationSetting
            {
                SourceSchemaName = "dbo",
                SourceTableName = "Users",
                SourceColumnName = "ClinicId",
                ReferenceSchemaName = "dbo",
                ReferenceTableName = "Clinics",
                ReferenceColumnName = "ClinicId"
            }
        };

        // Act
        var result = applier.Apply(new[] { childData }, settings);

        // Assert
        result[0].Rows[0]["ClinicId"].Is("Clinics_ClinicId_1");
    }

    private static GeneratedTestData CreateGeneratedData(
        string tableName,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows)
    {
        var table = new DbTableInfo
        {
            SchemaName = "dbo",
            TableName = tableName
        };
        var columns = rows[0].Keys
            .Select((columnName, index) => new DbColumnInfo
            {
                SchemaName = "dbo",
                TableName = tableName,
                ColumnName = columnName,
                DataType = "varchar",
                OrdinalPosition = index + 1
            })
            .ToList();

        return new GeneratedTestData(table, columns, rows);
    }

    private static IReadOnlyDictionary<string, string?> Row(params (string ColumnName, string? Value)[] values)
    {
        return values.ToDictionary(x => x.ColumnName, x => x.Value);
    }
}
