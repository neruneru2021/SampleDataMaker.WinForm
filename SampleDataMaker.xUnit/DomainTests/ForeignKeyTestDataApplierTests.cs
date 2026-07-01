using ChainingAssertion;
using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
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
        var applier = new ForeignKeyTestDataApplier(new TestSampleDataRepository());
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
        var applier = new ForeignKeyTestDataApplier(new TestSampleDataRepository());
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

    [TestMethod]
    public void カテゴリ項目の外部キーは参照先のレコードIDと同じカテゴリ値を引き継ぐ()
    {
        // Arrange
        var records = new[]
        {
            CategoryRecord(
                "person-001",
                ("苗字", "加藤"),
                ("苗字かな", "かとう")),
            CategoryRecord(
                "person-002",
                ("苗字", "佐藤"),
                ("苗字かな", "さとう"))
        };
        var applier = new ForeignKeyTestDataApplier(
            new TestSampleDataRepository(records));
        var parentData = CreateGeneratedData(
            tableName: "Parents",
            rows: new[]
            {
                Row(("LastName", "加藤"))
            },
            rowMetadata: new[]
            {
                Metadata(
                    0,
                    CategoryMetadata("LastName", "苗字", "person-001"))
            });
        var childData = CreateGeneratedData(
            tableName: "Children",
            rows: new[]
            {
                Row(("LastName", "佐藤"), ("LastNameKana", "さとう"))
            },
            rowMetadata: new[]
            {
                Metadata(
                    0,
                    CategoryMetadata("LastName", "苗字", "person-002"),
                    CategoryMetadata("LastNameKana", "苗字かな", "person-002"))
            });
        var settings = new[]
        {
            new ForeignKeyRelationSetting
            {
                SourceSchemaName = "dbo",
                SourceTableName = "Children",
                SourceColumnName = "LastName",
                ReferenceSchemaName = "dbo",
                ReferenceTableName = "Parents",
                ReferenceColumnName = "LastName"
            },
            new ForeignKeyRelationSetting
            {
                SourceSchemaName = "dbo",
                SourceTableName = "Parents",
                SourceColumnName = "LastName",
                ReferenceSchemaName = "dbo",
                ReferenceTableName = "Children",
                ReferenceColumnName = "LastName",
                IsReverse = true
            }
        };

        // Act
        var result = applier.Apply(new[] { parentData, childData }, settings);
        var appliedChildData = result.Single(x => x.Table.TableName == "Children");

        // Assert
        appliedChildData.Rows[0]["LastName"].Is("加藤");
        appliedChildData.Rows[0]["LastNameKana"].Is("かとう");
        appliedChildData.RowMetadata[0].Columns["LastName"].CategoryRecordId.Is("person-001");
        appliedChildData.RowMetadata[0].Columns["LastNameKana"].CategoryRecordId.Is("person-001");
        appliedChildData.RowMetadata[0].Columns["LastName"].IsForeignKeyInherited.IsTrue();
    }

    [TestMethod]
    public void 境界値カラムは外部キーとカテゴリの適用対象から除外する()
    {
        // Arrange
        var records = new[]
        {
            CategoryRecord(
                "person-001",
                ("苗字", "加藤"),
                ("苗字かな", "かとう"))
        };
        var applier = new ForeignKeyTestDataApplier(
            new TestSampleDataRepository(records));
        var parentData = CreateGeneratedData(
            tableName: "Parents",
            rows: new[]
            {
                Row(("LastName", "加藤"))
            },
            rowMetadata: new[]
            {
                Metadata(
                    0,
                    CategoryMetadata("LastName", "苗字", "person-001"))
            });
        var childData = CreateGeneratedData(
            tableName: "Children",
            rows: new[]
            {
                Row(("LastName", "ZZZZ"), ("LastNameKana", "さとう"))
            },
            rowMetadata: new[]
            {
                new GeneratedRowMetadata(
                    0,
                    new Dictionary<string, GeneratedColumnMetadata>
                    {
                        ["LastNameKana"] = CategoryMetadata(
                            "LastNameKana",
                            "苗字かな",
                            "person-001")
                    },
                    new HashSet<string>
                    {
                        "LastName"
                    })
            });
        var settings = new[]
        {
            new ForeignKeyRelationSetting
            {
                SourceSchemaName = "dbo",
                SourceTableName = "Children",
                SourceColumnName = "LastName",
                ReferenceSchemaName = "dbo",
                ReferenceTableName = "Parents",
                ReferenceColumnName = "LastName"
            }
        };

        // Act
        var result = applier.Apply(new[] { parentData, childData }, settings);
        var appliedChildData = result.Single(x => x.Table.TableName == "Children");

        // Assert
        appliedChildData.Rows[0]["LastName"].Is("ZZZZ");
    }

    private static GeneratedTestData CreateGeneratedData(
        string tableName,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> rows,
        IReadOnlyList<GeneratedRowMetadata>? rowMetadata = null)
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

        return new GeneratedTestData(table, columns, rows, rowMetadata);
    }

    private static IReadOnlyDictionary<string, string?> Row(params (string ColumnName, string? Value)[] values)
    {
        return values.ToDictionary(x => x.ColumnName, x => x.Value);
    }

    private static GeneratedRowMetadata Metadata(
        int rowIndex,
        params GeneratedColumnMetadata[] columns)
    {
        return new GeneratedRowMetadata(
            rowIndex,
            columns.ToDictionary(column => column.ColumnName));
    }

    private static GeneratedColumnMetadata CategoryMetadata(
        string columnName,
        string itemName,
        string recordId)
    {
        return new GeneratedColumnMetadata(
            columnName,
            "個人情報セット",
            itemName,
            recordId);
    }

    private static SampleDataCategoryRecord CategoryRecord(
        string id,
        params (string ItemName, string Value)[] values)
    {
        return new SampleDataCategoryRecord
        {
            Id = id,
            Values = values.ToDictionary(value => value.ItemName, value => value.Value)
        };
    }

    private sealed class TestSampleDataRepository : ISampleDataRepository
    {
        private readonly IReadOnlyList<SampleDataCategoryRecord> _records;

        public TestSampleDataRepository(
            IReadOnlyList<SampleDataCategoryRecord>? records = null)
        {
            _records = records ?? Array.Empty<SampleDataCategoryRecord>();
        }

        public IReadOnlyList<string> GetKinds() => Array.Empty<string>();

        public IReadOnlyList<string> GetValues(string kind) => Array.Empty<string>();

        public IReadOnlyList<SampleDataCategoryItem> GetCategoryItems() =>
            Array.Empty<SampleDataCategoryItem>();

        public IReadOnlyList<SampleDataCategoryRecord> GetCategoryRecords(string categoryName) =>
            categoryName == "個人情報セット"
                ? _records
                : Array.Empty<SampleDataCategoryRecord>();

        public bool TryGetCategoryRecord(
            string categoryName,
            string recordId,
            out SampleDataCategoryRecord? record)
        {
            record = GetCategoryRecords(categoryName)
                .FirstOrDefault(item => item.Id == recordId);

            return record != null;
        }
    }
}
