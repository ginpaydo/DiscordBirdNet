using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;

namespace DiscordBirdNet.Model
{
    /// <summary>
    /// このアプリのデータ構造を定義
    /// 変更したらマイグレーションする
    /// Add-Migration AddTestTableMigration(名前は毎回変更する)
    /// Update-Database
    /// また、DataAccessクラスも更新する
    /// </summary>
    class MyContext : DbContext
    {
        /// <summary>
        /// ユーザ情報
        /// </summary>
        public DbSet<Character> Characters { get; set; }

        /// <summary>
        /// とりっっちのパラメータ情報
        /// </summary>
        public DbSet<Parameter> Parameters { get; set; }

        /// <summary>
        /// 施設情報とレベル
        /// </summary>
        public DbSet<Facility> Facilities { get; set; }
        
        static private string s_migrationSqlitePath;
        static MyContext()
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            s_migrationSqlitePath = $@"{exeDir}data.sqlite3";
            //var exeDirInfo = new DirectoryInfo(exeDir);
            //var projectDir = exeDirInfo.Parent.Parent.FullName;
            //s_migrationSqlitePath = $@"{projectDir}\data.sqlite3";
        }

        public MyContext() : base(new SQLiteConnection($"DATA Source={s_migrationSqlitePath}"), false)
        {
        }

        public MyContext(DbConnection connection) : base(connection, true)
        {
        }
    }
}
