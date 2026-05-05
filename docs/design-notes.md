# SampleDataMaker 設計メモ

## WinForms と ViewModel の役割

- View は `DataGridView` など UI コントロールの設定とイベント入口だけを担当する。
- ViewModel は画面状態、表示データ、ユーザー操作の判断を担当する。
- ViewModel に `DataGridView` や `Form` など WinForms の具体クラスを持ち込まない。
- 画面遷移やダイアログ表示は interface 経由にする。
- DB 接続や SQL 文は Infrastructure に置く。
- ViewModel は Domain の interface を使い、Infrastructure の具体クラスを知らない。

## DataGridView の初期表示

`DataGridView.DataSource` を ViewModel の `BindingList<T>` にバインドした後で、ViewModel 側の `BindingList<T>` インスタンスを丸ごと差し替える場合は、`PropertyChanged` を発火させる必要がある。

NG 例:

```csharp
public BindingList<DbConnectionInfo> DgvConnectionsSource { get; private set; } = new();

// DataGridView 側に通知されない
DgvConnectionsSource = new BindingList<DbConnectionInfo>(connections.ToList());
```

OK 例:

```csharp
private BindingList<DbConnectionInfo> _dgvConnectionsSource = new();

public BindingList<DbConnectionInfo> DgvConnectionsSource
{
    get => _dgvConnectionsSource;
    private set => SetProperty(ref _dgvConnectionsSource, value);
}
```

これにより `DgvConnectionsSource` が差し替わったときに `PropertyChanged` が発火し、`DataGridView.DataSource` が更新される。

別案として、インスタンスを差し替えずに既存の `BindingList<T>` に `Clear()` / `Add()` する方法もある。ただし、バインド済みの `BindingList<T>` を別スレッドから更新すると UI スレッド例外になるため注意する。

## UI スレッドと SynchronizationContext

`new SynchronizationContext()` は WinForms の UI スレッドへ戻すものではない。

次のようにしてしまうと、`SynchronizationContext.Current` が `null` のときに通常の `SynchronizationContext` が入り、`Post()` しても UI スレッドに戻らない。

```csharp
_syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
```

ViewModel は UI スレッド上で生成し、UI 用の `WindowsFormsSynchronizationContext` を保持するのがよい。

```csharp
protected ViewModelBase()
{
    _syncContext = SynchronizationContext.Current
        ?? throw new InvalidOperationException(
            "ViewModelはUIスレッド上で生成してください。");
}
```

`SetProperty` では、現在スレッドが UI スレッドなら直接通知し、違う場合は `_syncContext.Post()` で UI スレッドへ戻す。

```csharp
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
```

## async Task を返す ViewModel メソッドの呼び方

ViewModel のメソッドが `Task` を返す場合、WinForms のイベントハンドラ側でも `await` する。

```csharp
Load += async (_, __) => await _vm.MainViewLoad();
RegisterButton.Click += async (_, __) => await _vm.Save();
```

`Task` を返すメソッドを次のように呼ぶと、呼びっぱなしになり、例外も拾いにくい。

```csharp
Load += (_, __) => _vm.MainViewLoad();
```

WinForms のイベントハンドラは戻り値が `void` のため、イベント入口だけは `async void` になってよい。

```csharp
private async void DgvConnections_CellContentClick(
    object? sender,
    DataGridViewCellEventArgs e)
{
    await _vm.DgvConnectionsCellContentClick(columnName, connection);
}
```

## DataGridView の Title カラムをクリック可能にする

左端に専用の「操作」ボタン列を作る代わりに、`Title` カラムを `DataGridViewLinkColumn` にすると、詳細画面への入口として自然に見せられる。

```csharp
private void AddOperationButtonColumn()
{
    var titleLinkColumn = new DataGridViewLinkColumn
    {
        Name = "Title",
        HeaderText = "Title",
        DataPropertyName = nameof(DbConnectionInfo.Title),
        TrackVisitedState = false,
        LinkBehavior = LinkBehavior.HoverUnderline,
        Width = 180
    };

    dgvConnections.Columns.Insert(0, titleLinkColumn);
}
```

`DataGridViewButtonColumn` にすれば、より明確にボタンとして表示できる。

```csharp
var titleButtonColumn = new DataGridViewButtonColumn
{
    Name = "Title",
    HeaderText = "Title",
    DataPropertyName = nameof(DbConnectionInfo.Title),
    UseColumnTextForButtonValue = false,
    Width = 180
};
```

## DataGridView のクリック処理を ViewModel に寄せる

`DataGridView` や `DataGridViewCellEventArgs` をそのまま ViewModel に渡すと、ViewModel が WinForms に強く依存して単体テストしづらくなる。

View 側は「クリックされた列名」と「行データ」を取り出すだけにする。

```csharp
private async void DgvConnections_CellContentClick(
    object? sender,
    DataGridViewCellEventArgs e)
{
    if (e.RowIndex < 0)
    {
        return;
    }

    var columnName = dgvConnections.Columns[e.ColumnIndex].Name;
    var connection = dgvConnections.Rows[e.RowIndex].DataBoundItem as DbConnectionInfo;

    await _vm.DgvConnectionsCellContentClick(columnName, connection);
}
```

ViewModel 側で「Title 列がクリックされたら操作画面を開く」という判断を行う。

```csharp
internal async Task DgvConnectionsCellContentClick(
    string columnName,
    DbConnectionInfo? connection)
{
    if (columnName != "Title")
    {
        return;
    }

    if (connection == null)
    {
        return;
    }

    await OpenOperationView(connection);
}
```

## 画面遷移は Navigator interface に逃がす

ViewModel から直接 `Form` を開くと、ViewModel が UI 実装に依存して単体テストが難しくなる。

画面遷移は interface にする。

```csharp
public interface IConnectionOperationNavigator
{
    Task Open(DbConnectionInfo connection);
}
```

ViewModel は interface だけを呼ぶ。

```csharp
public async Task OpenOperationView(DbConnectionInfo connection)
{
    await _operationNavigator.Open(connection);
}
```

WinForms 側の実装で実際に画面を開く。

```csharp
public class ConnectionOperationNavigator : IConnectionOperationNavigator
{
    public async Task Open(DbConnectionInfo connection)
    {
        var vm = DI.Resolve<ConnectionOperationViewModel>();

        using var view = new ConnectionOperationView(vm);

        await vm.Initialize(connection);

        view.ShowDialog();
    }
}
```

単体テストでは `IConnectionOperationNavigator` をモック化し、`Open(connection)` が呼ばれたことだけを確認できる。

## ConnectionOperationViewModel の作り方

`ConnectionOperationViewModel` のコンストラクタで `DbConnectionInfo` を直接受け取ると、DI コンテナが自動生成できない。

NG 例:

```csharp
public ConnectionOperationViewModel(DbConnectionInfo connection)
{
    _dbConnectionInfo = connection;
}
```

選択行データは DI で注入するものではなく、画面を開くタイミングで決まる実行時データなので、`Initialize()` で渡す。

```csharp
internal class ConnectionOperationViewModel : ViewModelBase
{
    private DbConnectionInfo? _dbConnectionInfo;

    public DbConnectionInfo? DbConnectionInfo
    {
        get => _dbConnectionInfo;
        private set => SetProperty(ref _dbConnectionInfo, value);
    }

    public async Task Initialize(DbConnectionInfo connection)
    {
        DbConnectionInfo = connection;

        // 必要な表示データをここで読み込む
    }
}
```

`ConnectionOperationView` は ViewModel を受け取る。

```csharp
public partial class ConnectionOperationView : Form
{
    private readonly ConnectionOperationViewModel _vm;

    internal ConnectionOperationView(ConnectionOperationViewModel vm)
    {
        InitializeComponent();

        _vm = vm;
    }
}
```

## SQL Server のテーブル一覧取得は Infrastructure に置く

DDD寄りにするなら、SQL Server への接続や SQL 文は `SampleDataMaker.Infrastructure` に集約する。

依存方向は次の形にする。

```text
WinForm/ViewModel
  ↓ Domain の interface
Domain
  ↑ interface の実装
Infrastructure
```

Domain 側に interface を置く。

```csharp
public interface IDbTableInfoRepository
{
    Task<IReadOnlyList<DbTableInfo>> GetTablesAsync(
        DbConnectionInfo connection,
        CancellationToken cancellationToken = default);
}
```

Domain 側に表示・処理で使うエンティティを置く。

```csharp
public class DbTableInfo
{
    public string SchemaName { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string DisplayName => $"{SchemaName}.{TableName}";
}
```

Infrastructure 側で SQL Server 用の実装を作る。

```csharp
public class SqlServerDbTableInfoRepository : IDbTableInfoRepository
{
    public async Task<IReadOnlyList<DbTableInfo>> GetTablesAsync(
        DbConnectionInfo connectionInfo,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo.DbType != DbTypeKind.SqlServer)
        {
            throw new NotSupportedException(
                $"{connectionInfo.DbType} は SQL Server 用Repositoryでは扱えません。");
        }

        var result = new List<DbTableInfo>();

        await using var connection =
            new SqlConnection(connectionInfo.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                TABLE_SCHEMA,
                TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY
                TABLE_SCHEMA,
                TABLE_NAME
            """;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new DbTableInfo
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
            });
        }

        return result;
    }
}
```

WinForm の DI で interface と実装を紐づける。

```csharp
_services.AddTransient<IDbTableInfoRepository, SqlServerDbTableInfoRepository>();
```

ViewModel は SQL Server の接続方法や SQL 文を知らず、Domain の interface だけを使う。

```csharp
internal class ConnectionOperationViewModel : ViewModelBase
{
    private readonly IDbTableInfoRepository _dbTableInfoRepository;

    private BindingList<DbTableInfo> _tablesSource = new();

    public BindingList<DbTableInfo> TablesSource
    {
        get => _tablesSource;
        private set => SetProperty(ref _tablesSource, value);
    }

    public ConnectionOperationViewModel(
        IDbTableInfoRepository dbTableInfoRepository)
    {
        _dbTableInfoRepository = dbTableInfoRepository;
    }

    public async Task Initialize(DbConnectionInfo connection)
    {
        var tables = await _dbTableInfoRepository.GetTablesAsync(connection);

        TablesSource = new BindingList<DbTableInfo>(tables.ToList());
    }
}
```

## ConnectionOperationView の DataGridView バインド

`ConnectionOperationView` 側では、`DataGridView` の列定義と DataSource バインドだけを行う。

```csharp
internal ConnectionOperationView(ConnectionOperationViewModel vm)
{
    InitializeComponent();

    _vm = vm;

    SetupDgvTables();

    dgvTables.DataBindings.Add(
        nameof(dgvTables.DataSource),
        _vm,
        nameof(_vm.TablesSource));
}
```

列定義の例:

```csharp
private void SetupDgvTables()
{
    dgvTables.AutoGenerateColumns = false;
    dgvTables.Columns.Clear();

    dgvTables.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "SchemaName",
        HeaderText = "Schema",
        DataPropertyName = nameof(DbTableInfo.SchemaName),
        Width = 120
    });

    dgvTables.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "TableName",
        HeaderText = "Table",
        DataPropertyName = nameof(DbTableInfo.TableName),
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    });
}
```

## 命名と配置の目安

WinForms 固有の画面遷移 interface は `SampleDataMaker.WinForm/Services` に置く。

```text
SampleDataMaker.WinForm/
  Services/
    IConnectionOperationNavigator.cs
    ConnectionOperationNavigator.cs
```

データアクセスの interface は `SampleDataMaker.Domain/Repositories` に置く。

```text
SampleDataMaker.Domain/
  Repositories/
    IDbTableInfoRepository.cs
```

SQL Server など具体的な DB 接続実装は `SampleDataMaker.Infrastructure` に置く。

```text
SampleDataMaker.Infrastructure/
  Database/
    SqlServerDbTableInfoRepository.cs
```

ViewModel は `WinForm` プロジェクトに置き、UI の状態とユーザー操作の判断を持つ。ただし `DataGridView` や `Form` そのものは持ち込まない。

```text
SampleDataMaker.WinForm/
  ViewModels/
    MainViewModel.cs
    ConnectionOperationViewModel.cs
```

## 現時点の設計方針

- View は UI コントロールの設定とイベント入口だけを担当する。
- ViewModel は画面状態、表示データ、ユーザー操作の判断を担当する。
- 画面を開く処理は Navigator interface 経由にする。
- DB 接続や SQL 文は Infrastructure に置く。
- ViewModel は Domain の interface を使い、Infrastructure の具体クラスを知らない。
- 実行時に決まる選択行データは、DI コンストラクタではなく `Initialize()` で渡す。
- `Task` を返す処理は呼び出し側で `await` する。
- UI バインド済みのコレクションを別スレッドから直接変更しない。

## サンプルデータマスタの方針

テストデータ生成で、文字列なら `A`、数字なら `1` のような固定値だけではなく、名前・電話番号・住所などの実データに近い値を使えるようにしたい。

候補としては次の案がある。

| 案 | 評価 |
| --- | --- |
| SQLite | 検索、大量データ、画面編集に強い。ただし初期実装としては少し重い。 |
| JSON | 更新、バックアップ、配布、Git管理が簡単。今の要件に一番合う。 |
| Excel | 非エンジニアが編集しやすい。ただし読み込み実装とファイル破損時の扱いが面倒。 |
| C# 定数 | 最初は楽だが、更新のたびにビルドが必要。今回は避ける。 |

現時点のおすすめは **JSON ファイルを標準マスタにする** 方針。

```text
SampleDataMaker.WinForm/
  master-data/
    sample-data.json
```

JSON は、将来 SQLite に移行しやすいように、DB テーブルに近い配列形式にする。

```json
[
  { "kind": "名前", "value": "佐藤" },
  { "kind": "名前", "value": "伊藤" },
  { "kind": "名前", "value": "加藤" },
  { "kind": "電話番号", "value": "090-8888-8888" },
  { "kind": "電話番号", "value": "090-8888-8882" }
]
```

`kind` がデータ種類、`value` が実際に使う値。

テストデータ作成画面では、テーブルのカラム一覧に対して、サンプルデータマスタを使うかどうかを選択できるようにする。

イメージ:

| Column | DataType | サンプル使用 | 種類 |
| --- | --- | --- | --- |
| CustomerName | nvarchar | ON | 名前 |
| Tel | varchar | ON | 電話番号 |
| Price | int | OFF |  |

`サンプル使用` が ON の場合、選択した `種類` に紐づく値を順番またはランダムに使う。

例:

```text
種類 = 名前
値 = 佐藤, 伊藤, 加藤
```

この場合、生成されるテストデータには `佐藤`、`伊藤`、`加藤` が使われる。

## サンプルデータマスタの配置

DDD寄りにするなら、サンプルデータマスタの読み込み interface は Domain 側に置き、JSON の読み込み実装は Infrastructure 側に置く。

```text
Domain
  Entities
    SampleDataItem.cs
  Repositories
    ISampleDataRepository.cs

Infrastructure
  Json
    JsonSampleDataRepository.cs
```

Domain のエンティティ例:

```csharp
public class SampleDataItem
{
    public string Kind { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
```

Domain の repository interface 例:

```csharp
public interface ISampleDataRepository
{
    IReadOnlyList<string> GetKinds();

    IReadOnlyList<string> GetValues(string kind);
}
```

Infrastructure の JSON 実装例:

```csharp
public class JsonSampleDataRepository : ISampleDataRepository
{
    private readonly string _filePath;
    private List<SampleDataItem> _items = new();

    public JsonSampleDataRepository()
    {
        _filePath = Path.Combine(
            AppContext.BaseDirectory,
            "master-data",
            "sample-data.json");

        Load();
    }

    public IReadOnlyList<string> GetKinds()
    {
        return _items
            .Select(x => x.Kind)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    public IReadOnlyList<string> GetValues(string kind)
    {
        return _items
            .Where(x => x.Kind == kind)
            .Select(x => x.Value)
            .ToList();
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            _items = new List<SampleDataItem>();
            return;
        }

        var json = File.ReadAllText(_filePath);
        _items = JsonSerializer.Deserialize<List<SampleDataItem>>(json) ?? new();
    }
}
```

## サンプルデータマスタの将来拡張

最初は JSON を標準マスタにする。

将来的に、利用者が Excel や CSV で編集したい場合は、JSON を直接やめるのではなく、Excel/CSV から JSON に取り込むインポート機能を追加するのがよい。

```text
Excel/CSV
  ↓ インポート
sample-data.json
  ↓ ISampleDataRepository
テストデータ生成
```

こうしておくと、アプリ内部は `ISampleDataRepository` だけを見ればよく、保存形式を後から変えても生成ロジックに影響しにくい。

結論:

- 最初は JSON。
- 編集画面や検索性が必要になったら SQLite を検討する。
- Excel/CSV は補助的なインポート機能として後から追加する。
- C# 定数で大量に持つ案は、更新のたびにビルドが必要になるため避ける。
