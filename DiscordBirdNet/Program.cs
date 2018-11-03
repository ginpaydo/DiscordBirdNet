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
