using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBirdNet
{
    /// <summary>
    /// botのステータス
    /// </summary>
    public class Status
    {
        #region フィールド
        // 名前
        const string StrName = "Name";
        public string Name
        {
            get { return appData.Parameters[StrName].Value; }
            set { appData.Parameters[StrName].Value = value; }
        }

        // 最大値
        const string StrMaxHp = "MaxHp";
        const string StrMaxMp = "MaxMp";
        const string StrMaxUnko = "MaxUnko";
        public int MaxHp
        {
            get { return int.Parse(appData.Parameters[StrMaxHp].Value); }
            set { appData.Parameters[StrMaxHp].Value = value.ToString(); }
        }
        public int MaxMp
        {
            get { return int.Parse(appData.Parameters[StrMaxMp].Value); }
            set { appData.Parameters[StrMaxMp].Value = value.ToString(); }
        }
        public int MaxUnko
        {
            get { return int.Parse(appData.Parameters[StrMaxUnko].Value); }
            set { appData.Parameters[StrMaxUnko].Value = value.ToString(); }
        }

        // 変動
        const string StrHp = "Hp";
        const string StrMp = "Mp";
        const string StrUnko = "Unko";
        const string StrMoney = "Money";
        const string StrIncome = "Income";
        public int Hp
        {
            get { return int.Parse(appData.Parameters[StrHp].Value); }
            set { appData.Parameters[StrHp].Value = value.ToString(); }
        }
        public int Mp
        {
            get { return int.Parse(appData.Parameters[StrMp].Value); }
            set { appData.Parameters[StrMp].Value = value.ToString(); }
        }
        public int Unko
        {
            get { return int.Parse(appData.Parameters[StrUnko].Value); }
            set { appData.Parameters[StrUnko].Value = value.ToString(); }
        }
        public BigInteger Money
        {
            get { return BigInteger.Parse(appData.Parameters[StrMoney].Value); }
            set { appData.Parameters[StrMoney].Value = value.ToString(); }
        }
        public BigInteger Income
        {
            get { return BigInteger.Parse(appData.Parameters[StrIncome].Value); }
            set { appData.Parameters[StrIncome].Value = value.ToString(); }
        }

        //ステータス異常（必要なら後でパラメータ化）
        public bool isDead;

        // 一緒に買い物している人
        public string BuyWith = string.Empty;
        public BigInteger budget = 0;

        // 好感度などのデータ
        public AppData appData;

        // データベース
        DataAccess db;
        #endregion

        #region 初期化
        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            // データベースのセットアップと読み込み
            db = new DataAccess();
            appData = db.Initialize();

            appData.InitializeParamater(StrName, "とりっっち");
            appData.InitializeParamater(StrMaxHp, "100");
            appData.InitializeParamater(StrMaxMp, "100");
            appData.InitializeParamater(StrMaxUnko, "50");
            appData.InitializeParamater(StrHp, "50");
            appData.InitializeParamater(StrMp, "80");
            appData.InitializeParamater(StrUnko, "50");
            appData.InitializeParamater(StrMoney, "146740");
            appData.InitializeParamater(StrIncome, "0");

            appData.InitializeFacility("ピーナッツ栽培", 100, 5, "ピーナッツを栽培します。");
            appData.InitializeFacility("ソフトクリームの製造", 250, 10, "ソフトクリームを作ります。");
            appData.InitializeFacility("からあげクンの製造", 500, 20, "からあげクンを作ります。");
            appData.InitializeFacility("カレーライスの製造", 1000, 40, "カレーライスを作ります。");
            appData.InitializeFacility("蛇口", 2000, 80, "仮想通貨を恵んでもらいます。");
            appData.InitializeFacility("おばあちゃん", 4000, 160, "おばあちゃんに手伝ってもらいます。クッキー作りが得意だそうです。");
            appData.InitializeFacility("ブログ", 8000, 300, "ブログで情報を流して儲けます。");
            appData.InitializeFacility("せどり", 16000, 600, "フリマアプリを使って安く仕入れて売ります。");
            appData.InitializeFacility("とらっぴぃ", 32000, 1200, "最強の仮想通貨自動売買BOTを購入して運用します。");
            appData.InitializeFacility("ゲーム", 60000, 2400, "Webに自作ゲームを公開して情弱に課金してもらいます。");
            appData.InitializeFacility("マイニングファーム", 120000, 4800, "マイニング事業に投資します。");
            appData.InitializeFacility("ロケット工場", 240000, 9000, "ロケット工場を買収して、製造プラントを確保します。");

            isDead = false;
        }
        #endregion

        #region 更新
        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (isDead)
            {
                Hp = Math.Min(Hp + 1, MaxHp);
                if (Hp == MaxHp)
                {
                    isDead = false;
                }
            }
            else
            {
                Mp = Math.Min(Mp + 1, MaxMp);
                Unko = Math.Min(Unko + 1, MaxUnko);
                Money += Income;
            }
        }
        #endregion

        #region 好感度操作
        /// <summary>
        /// 好感度の増減
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void CulcLike(string name, int value)
        {
            int current = value;
            if (appData.Characters.ContainsKey(name))
            {
                current += appData.Characters[name].Like;
            }
            else
            {
                appData.Characters.Add(name, new Model.Character() { Name = name });
            }
            appData.Characters[name].Like = current;

            db.Save(appData);  // TODO:test
        }

        /// <summary>
        /// 好感度の初期化
        /// </summary>
        /// <param name="name"></param>
        public void ResetLike(string name)
        {
            appData.Characters[name].Like = 0;

            db.Save(appData);  // TODO:test
        }

        #endregion

        #region ステータス表示
        public string ShowStatus()
        {
            // 好きな人
            string like = "いない";
            int maxLike = 0;
            foreach (var item in appData.Characters.Keys)
            {
                int current = appData.Characters[item].Like;
                if (current > 0)
                {
                    if (maxLike < current)
                    {
                        maxLike = current;
                        like = item;
                    }
                }
            }

            // 結果
            StringBuilder sb = new StringBuilder();
            sb.Append("名前: " + Name + "\n");
            sb.Append("HP: " + Hp + "/" + MaxHp + "\n");
            sb.Append("MP: " + Mp + "/" + MaxMp + "\n");
            sb.Append("好きな人: " + like + "\n");
            return sb.ToString();
        }

        public string ShowStatusAdvance()
        {
            // 好きな人
            string like = "いない";
            int maxLike = 0;
            foreach (var item in appData.Characters.Keys)
            {
                int current = appData.Characters[item].Like;
                if (current > 0)
                {
                    if (maxLike < current)
                    {
                        maxLike = current;
                        like = item;
                    }
                }
            }

            // 結果
            StringBuilder sb = new StringBuilder();
            sb.Append("名前: " + Name + "\n");
            sb.Append("HP: " + Hp + "/" + MaxHp + "\n");
            sb.Append("MP: " + Mp + "/" + MaxMp + "\n");
            sb.Append("収入: " + Income + "\n");
            sb.Append($"所持金: {Money}円\n");
            sb.Append("好きな人: " + like + "\n");
            return sb.ToString();
        }
        #endregion
    }
}
