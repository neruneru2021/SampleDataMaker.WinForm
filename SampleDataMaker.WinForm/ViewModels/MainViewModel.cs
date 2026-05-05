using SampleDataMaker.Domain.Entities;
using SampleDataMaker.Domain.Repositories;
using SampleDataMaker.WinForm.Services;
using System.ComponentModel;

namespace SampleDataMaker.WinForm.ViewModels
{
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

        public MainViewModel(
            IDbConnectionInfoRepository dbConnectionInfoRepository,
            IConnectionOperationNavigator operationNavigator)
        {
            _dbConnectionInfoRepository = dbConnectionInfoRepository;
            _operationNavigator = operationNavigator;
        }

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

        public async Task OpenOperationView(DbConnectionInfo connection)
        {
            await _operationNavigator.Open(connection);
        }

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
