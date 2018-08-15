using DiscordBirdNet.Migrations;
using DiscordBirdNet.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SQLite;
using System.Reflection;

namespace DiscordBirdNet
{
    /// <summary>
    /// データを読み書きするためのクラス
    /// テーブルを追加した場合は、LoadDataとSaveにも追加すること
    /// </summary>
    class DataAccess
    {
        const string dbName = "data.sqlite3";

        public AppData Initialize()
        {
            // データモデル変更をDBに反映する
            Migration();

            // データ読み込み
            return LoadData();
        }


        #region テーブルを追加した場合に修正が必要な部分
        /// <summary>
        /// 全てのデータ読み込み
        /// </summary>
        /// <returns>データ</returns>
        public AppData LoadData()
        {
            var appData = new AppData();

            using (MyContext context = new MyContext())
            {
                // キャラクタ一覧
                var charactersData = new Dictionary<string, Character>();
                var characters = context.Characters;
                foreach (var item in characters)
                {
                    charactersData.Add(item.Name, item);
                }
                appData.Characters = charactersData;

                // とりっっちのパラメータ
                var parametersData = new Dictionary<string, Parameter>();
                var parameters = context.Parameters;
                foreach (var item in parameters)
                {
                    parametersData.Add(item.Name, item);
                }
                appData.Parameters = parametersData;

                // 施設一覧
                var facilitiesData = new Dictionary<string, Facility>();
                var facilities = context.Facilities;
                foreach (var item in facilities)
                {
                    facilitiesData.Add(item.Name, item);
                }
                appData.Facilities = facilitiesData;
            }

            return appData;
        }

        /// <summary>
        /// データを保存する（追加または更新のみ）
        /// </summary>
        /// <param name="appData">データ</param>
        public void Save(AppData appData)
        {
            using (MyContext context = new MyContext())
            {
                // キャラクタ一覧
                var characters = appData.Characters;
                foreach (var item in characters.Values)
                {
                    context.Set<Character>().AddOrUpdate(item);
                }

                // とりっっちのパラメータ
                var parameters = appData.Parameters;
                foreach (var item in parameters.Values)
                {
                    context.Set<Parameter>().AddOrUpdate(item);
                }

                // 施設一覧
                var facilities = appData.Facilities;
                foreach (var item in facilities.Values)
                {
                    context.Set<Facility>().AddOrUpdate(item);
                }

                #region 削除方法
                //// 削除
                //var deleteTarget = context.Characters.Single(x => x.Id == 4);
                //context.Characters.Remove(deleteTarget);
                #endregion

                context.SaveChanges();
            }
        }
        #endregion

        #region Migration：マイグレーション
        /// <summary>
        /// マイグレーション
        /// データモデルを変更したら、"Add-Migration AddTestTableMigration"を行うとDBに反映される
        /// </summary>
        private static void Migration()
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = $"{exeDir}{dbName}";
            var connStr = $"DATA Source={dbPath}";

            using (var connection = new SQLiteConnection(connStr))
            {
                using (var context = new MyContext(connection))
                {
                    // providerNameをコードを使って取得する。
                    // コードを使わずに、直接"System.Data.SQLite"を使ってもいい
                    // https://stackoverflow.com/questions/36060478/dbmigrator-does-not-detect-pending-migrations-after-switching-database
                    var internalContext = context.GetType().GetProperty("InternalContext", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(context);
                    var providerName = (string)internalContext.GetType().GetProperty("ProviderName").GetValue(internalContext);

                    // Migratorが使うConfigurationを生成する。
                    // TargetDatabaseはDbMigratorの方ではなく、Configurationの方に設定しないと効果が無い。
                    var configuration = new Configuration()
                    {
                        TargetDatabase = new DbConnectionInfo(context.Database.Connection.ConnectionString, providerName)
                    };

                    // DbMigratorを生成する
                    var migrator = new DbMigrator(configuration);

                    // EF6.13では問題ないが、EF6.2の場合にUpdateのタイミングで以下の例外が吐かれないようにする対策
                    // System.ObjectDisposedException: '破棄されたオブジェクトにアクセスできません。
                    // オブジェクト名 'SQLiteConnection' です。'
                    // https://stackoverflow.com/questions/47329496/updating-to-ef-6-2-0-from-ef-6-1-3-causes-cannot-access-a-disposed-object-error/47518197
                    var _historyRepository = migrator.GetType().GetField("_historyRepository", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(migrator);
                    var _existingConnection = _historyRepository.GetType().BaseType.GetField("_existingConnection", BindingFlags.Instance | BindingFlags.NonPublic);
                    _existingConnection.SetValue(_historyRepository, null);

                    // Migrationを実行する。
                    migrator.Update();
                }
            }
        }
        #endregion

        #region 没コード（ノートの代わり）
        private void ShowCharactors(SQLiteConnectionStringBuilder db)
        {
            // var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = $"{dbName}" };
            using (var cn = new SQLiteConnection(db.ToString()))
            {
                string sql;
                SQLiteCommand command;
                cn.Open();

                // テーブルが無かったら作成
                sql = "create table if not exists characters (id integer primary key, name varchar(100), like integer);";
                command = new SQLiteCommand(sql, cn);
                command.ExecuteNonQuery();

                // キャラクター一覧
                using (var cmd = new SQLiteCommand(cn))
                {
                    cmd.CommandText = "select * from characters order by id;";
                    Console.WriteLine(cmd.ExecuteScalar());
                }
            }
        }
        #endregion

    }
}
