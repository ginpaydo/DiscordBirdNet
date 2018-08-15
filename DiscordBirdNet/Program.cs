using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Discord;
using DiscordBirdNet.Model;
using System.Configuration;

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
        public int Money
        {
            get { return int.Parse(appData.Parameters[StrMoney].Value); }
            set { appData.Parameters[StrMoney].Value = value.ToString(); }
        }
        public int Income
        {
            get { return int.Parse(appData.Parameters[StrIncome].Value); }
            set { appData.Parameters[StrIncome].Value = value.ToString(); }
        }

        //ステータス異常（必要なら後でパラメータ化）
        public bool isDead;

        // 一緒に買い物している人
        public string BuyWith = string.Empty;
        public int budget = 0;

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
            appData.InitializeParamater(StrHp, "30");
            appData.InitializeParamater(StrMp, "100");
            appData.InitializeParamater(StrUnko, "50");
            appData.InitializeParamater(StrMoney, "146740");
            appData.InitializeParamater(StrIncome, "10");

            appData.InitializeFacility("ピーナッツ栽培", 100, 10, "ピーナッツを栽培します。");
            appData.InitializeFacility("ソフトクリームの製造", 250, 20, "ソフトクリームを作ります。");
            appData.InitializeFacility("からあげクンの製造", 500, 40, "からあげクンを作ります。");
            appData.InitializeFacility("カレーライスの製造", 1000, 80, "カレーライスを作ります。");
            appData.InitializeFacility("蛇口", 2000, 160, "仮想通貨を恵んでもらいます。");
            appData.InitializeFacility("おばあちゃん", 4000, 320, "おばあちゃんに手伝ってもらいます。クッキー作りが得意だそうです。");
            appData.InitializeFacility("ブログ", 8000, 600, "ブログで情報を流して儲けます。");
            appData.InitializeFacility("せどり", 16000, 1200, "フリマアプリを使って安く仕入れて売ります。");
            appData.InitializeFacility("とらっぴぃ", 32000, 2400, "最強の仮想通貨自動売買BOTを購入して運用します。");
            appData.InitializeFacility("ゲーム", 60000, 4800, "Webに自作ゲームを公開して情弱に課金してもらいます。");
            appData.InitializeFacility("マイニングファーム", 120000, 9600, "マイニング事業に投資します。");
            appData.InitializeFacility("ロケット工場", 240000, 18000, "ロケット工場を買収して、開発プラントを確保します。");

            isDead = false;
        }
        #endregion


        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (isDead)
            {
                Hp = Math.Min(Hp + 1, MaxHp);
                if(Hp == MaxHp)
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
                appData.Characters.Add(name, new Model.Character() { Name = name});
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
                    if(maxLike < current)
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
    }

    class Program
    {
        public static Char[] trimList = new Char[] { ' ', '、', ',', '。', '\n', '　', '・', '･', '.' };
        public static Status toricchiStatus;

        public static DiscordSocketClient client;
        public static CommandService commands;
        public static IServiceProvider services;

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        /* コマンド列挙 */
        public Dictionary<string, string> commandList = new Dictionary<string, string>()
        {
            {"help", "コマンドの説明をしてくれます。"},
            {"shoottori", "とりっっちを撃ちます。"},
            {"buyitem", "とりっっちと楽しいお買い物をします。"},
            {"inventory", "とりっっちの持ち物を見せて貰います。"},
            {"ステータス", "とりっっちの秘密が見えちゃいます。"},
            {"テスト", "試験のアナウンスをします。"},
            {"あんあんあん", "愛し合うことができます。"},
            {"お前を消す方法", "使わないでください。"},
            {"おにいちゃん", "おにいちゃんにおまかせ。"},
            {"デジタルメガフレア", "MPを100消費する。相手は死ぬ。"}
            //{ "!join", "俺を召喚できる。"},
            //{ "!song", "？？？"},
            //{ "!deteike", "俺を追い出してしまう。"}
        };

        public Dictionary<string, string> botkeywordList = new Dictionary<string, string>()
        {
            {"今日の最高値",""}
        };

        public Dictionary<string, string> keywordList = new Dictionary<string, string>()
        {
            {"はらへった","動物専用コマンド。"},
            {"腹減った","動物専用コマンド。"},
            {"おなかすいた","動物専用コマンド。"},
            {"お腹空いた","動物専用コマンド。"},
            {"うんこ","動物専用コマンド。"},
            {"下痢","動物専用コマンド。"}
        };

        /// <summary>
        /// 起動時処理
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            toricchiStatus = new Status();
            toricchiStatus.Initialize();
            Messages.Initialize(commandList);
            client = new DiscordSocketClient();
            commands = new CommandService();
            services = new ServiceCollection().BuildServiceProvider();
            client.MessageReceived += CommandRecieved;

            client.Log += Log;
            string token = ConfigurationManager.AppSettings["Token"];
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        /// <summary>
        /// 収入を計算する
        /// </summary>
        /// <returns></returns>
        int CalculateIncome()
        {
            int sum = 0;
            foreach (var item in toricchiStatus.appData.Facilities.Values)
            {
                sum += item.CurrentIncome;
            }
            
            return sum;
        }

        /// <summary>
        /// メッセージの受信処理
        /// </summary>
        /// <param name="msgParam"></param>
        /// <returns></returns>
        private async Task CommandRecieved(SocketMessage messageParam)
        {
            if (!toricchiStatus.isDead)
            {
                var message = messageParam as SocketUserMessage;
                Messages.UpdateMessage(message, toricchiStatus, client);

                // message.Channel.Name         voicechat-2
                // message.Author.Username      てばさき
                // message:                     実際のメッセージ
                Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message.ToString());

                // メッセージが入っているか
                if (message == null) { return; }
                // コメントがユーザーかBotかの判定
                if (message.Author.IsBot)
                {
                    #region BOT相手
                    string keyword = null;

                    // メッセージからスペースとかを除外
                    string temp = message.ToString();
                    temp = trimList.Aggregate(temp, (s, c) => s.Replace(c.ToString(), ""));

                    // メッセージにコマンドが含まれているか
                    foreach (var item in botkeywordList.Keys)
                    {
                        if (temp.Contains(item))
                        {
                            // ./コマンドは、文頭のみ反応
                            if (item.StartsWith("./"))
                            {
                                if (temp.StartsWith(message.ToString()))
                                {
                                    continue;
                                }
                            }

                            keyword = item;
                            break;
                        }
                    }
                    if (keyword != null)
                    {
                        // キーワードが含まれていた場合

                        // 実行
                        var context = new CommandContext(client, message);
                        var result = await commands.ExecuteAsync(context, keyword, services);

                        //実行できなかった場合
                        if (!result.IsSuccess) { await context.Channel.SendMessageAsync(result.ErrorReason); }
                    }
                    #endregion
                }
                else
                {
                    // BOTではない場合
                    #region キーワード処理
                    string keyword = null;

                    if (!string.IsNullOrEmpty(toricchiStatus.BuyWith) && toricchiStatus.BuyWith == message.Author.Username)
                    {
                        int id = 0;
                        if (int.TryParse(message.ToString(), out id))
                        {
                            // お買い物
                            var item = toricchiStatus.appData.Facilities.Values.FirstOrDefault(f => f.Id == id && f.CurrentPrice < toricchiStatus.budget);
                            if (item != null)
                            {
                                // お金減少
                                toricchiStatus.Money -= item.CurrentPrice;
                                // アイテム状態更新
                                item.Level++;
                                item.CurrentPrice = (int)(item.CurrentPrice * 1.1);
                                item.CurrentIncome = item.Level * item.BaseIncome;
                                // 収入変更
                                toricchiStatus.Income = CalculateIncome();
                                var context = new CommandContext(client, message);
                                var result = await commands.ExecuteAsync(context, "!toricchiItemSuccess", services);
                            }
                            else
                            {
                                var context = new CommandContext(client, message);
                                var result = await commands.ExecuteAsync(context, "!toricchiItemFailure", services);
                            }
                        }
                        else
                        {
                            var context = new CommandContext(client, message);
                            var result = await commands.ExecuteAsync(context, "!toricchiItemFailure", services);
                        }

                        toricchiStatus.BuyWith = string.Empty;
                        toricchiStatus.budget = 0;
                    }
                    else
                    {
                        // お買い物ではない
                        // メッセージからスペースとかを除外
                        string temp = message.ToString();
                        temp = trimList.Aggregate(temp, (s, c) => s.Replace(c.ToString(), ""));
                        foreach (var item in keywordList.Keys)
                        {
                            if (temp.Contains(item))
                            {
                                keyword = item;
                                break;
                            }
                        }
                        if (keyword != null)
                        {
                            // キーワードが含まれていた場合

                            // 実行
                            var context = new CommandContext(client, message);
                            var result = await commands.ExecuteAsync(context, keyword, services);

                            ////実行できなかった場合
                            //if (!result.IsSuccess) { await context.Channel.SendMessageAsync(result.ErrorReason); }
                        }
                        else
                        {
                            // キーワードが含まれていなかった場合
                            // 通常の命令実行
                            var context = new CommandContext(client, message);
                            var result = await commands.ExecuteAsync(context, message.ToString(), services);

                            ////実行できなかった場合
                            //if (!result.IsSuccess) { await context.Channel.SendMessageAsync(result.ErrorReason); }
                        }
                    }
                    #endregion
                }
            }
            toricchiStatus.Update();
        }

        /// <summary>
        /// コンソール表示処理
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
