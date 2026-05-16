using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.WinForm.Services;
using System.ComponentModel;

namespace SampleDataMaker.WinForm.ViewModels
{
    /// <summary>
    /// 接続先一覧画面の読み込み、保存、操作画面への遷移を管理します。
    /// </summary>
    internal class MainViewModel : ViewModelBase
    {
        private readonly IDbConnectionInfoRepository _dbConnectionInfoRepository;
        private readonly IConnectionOperationNavigator _operationNavigator;

        private BindingList<DbConnectionInfo> _dgvConnectionsSource = new();

        public BindingList<DbConnectionInfo> DgvConnectionsSource
        {
            get => _dgvConnectionsSource;
            private set => SetProperty(ref _dgvConnectionsSource, value);
        }

        /// <summary>
        /// 接続情報リポジトリと画面遷移サービスを受け取ります。
        /// </summary>
        public MainViewModel(
            IDbConnectionInfoRepository dbConnectionInfoRepository,
            IConnectionOperationNavigator operationNavigator)
        {
            _dbConnectionInfoRepository = dbConnectionInfoRepository;
            _operationNavigator = operationNavigator;
        }

        /// <summary>
        /// 保存済みのDB接続情報を読み込み、一覧表示用ソースに反映します。
        /// </summary>
        internal async Task MainViewLoad()
        {
            try
            {
                // BindingList<T> に Add / Clear する場合
                // DataGridView にバインド済みの BindingList を別スレッドから直接変更すると、例外が出ます。
                // 別スレッドで取得だけして、UI スレッドへ戻ってから DgvConnectionsSource を差し替える、という形が安全です。
                // この形なら、await 後に UI スレッドへ戻るので自然に安全です。
                var connections = await Task.Run(() => _dbConnectionInfoRepository.GetAll());

                DgvConnectionsSource = new BindingList<DbConnectionInfo>(connections.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"DB接続情報の読み込みに失敗しました。\r\n\r\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 画面上で編集されたDB接続情報を保存します。
        /// </summary>
        internal async Task Save()
        {
            try
            {
                await Task.Run(() => _dbConnectionInfoRepository.SaveAll(DgvConnectionsSource.ToList()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"DB接続情報の保存に失敗しました。\r\n\r\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 指定された接続情報でテーブル操作画面を開きます。
        /// </summary>
        public async Task OpenOperationView(DbConnectionInfo connection)
        {
            await _operationNavigator.Open(connection);
        }

        /// <summary>
        /// 接続一覧のタイトルリンクがクリックされたときに操作画面へ遷移します。
        /// </summary>
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
    }
}
