using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading;

namespace DiscordBirdNet
{
    /// <summary>
    /// とりっっちの受け答えの定義
    /// </summary>
    public class Messages : ModuleBase
    {
        #region フィールド
        /// <summary>
        /// コマンドのリスト
        /// Programから初期化する
        /// </summary>
        public static Dictionary<string, string> commandList;

        /// <summary>
        /// 受信メッセージ
        /// </summary>
        private static SocketUserMessage SocketUserMessage;

        /// <summary>
        /// ステータス
        /// </summary>
        private static Status ToricchiStatus;

        /// <summary>
        /// Discordのクライアント
        /// </summary>
        private static DiscordSocketClient Client;

        /// <summary>
        /// 乱数
        /// </summary>
        private static Random random;
        #endregion

        #region メッセージ受信
        public static void UpdateMessage(SocketUserMessage Message, Status toricchiStatus, DiscordSocketClient client)
        {
            SocketUserMessage = Message;
            ToricchiStatus = toricchiStatus;
            Client = client;
        }
        #endregion

        #region 初期化
        public static void Initialize(Dictionary<string, string> list)
        {
            commandList = list;
            random = new Random();
        }
        #endregion

        #region 没
        ///// <summary>
        ///// [!join]というコメントが来た際の処理
        ///// </summary>
        ///// <returns>Botのコメント</returns>
        //[Command("!join", RunMode = RunMode.Async)]
        //public async Task Summon()
        //{
        //    var user = SocketUserMessage.Author.Username;

        //    // ボイチャ
        //    //IGuild guild = Context.Guild;
        //    //Console.WriteLine(SocketUserMessage.Channel.Name);  // test
        //    //Console.WriteLine(SocketUserMessage.Source.ToString()); // User
        //    //Console.WriteLine(voiceChannel.Guild);  // ぎんぺーラボ
        //    //Console.WriteLine(voiceChannel.Name);  // General

        //    ToricchiStatus.VoiceChannel = ((IVoiceState)Context.User).VoiceChannel;

        //    if (ToricchiStatus.VoiceChannel == null)
        //    {
        //        await ReplyAsync($"おーい{user}！どこにいるんだ？\n俺を一羽にしないでくれよぉ:cry:");
        //    }
        //    else
        //    {
        //        await ReplyAsync($"何だ{user}？\n俺に何か用か？");
        //        ToricchiStatus.AudioClient = await ToricchiStatus.VoiceChannel.ConnectAsync();    // これで繋がる
        //    }
        //}

        //[Command("!deteike", RunMode = RunMode.Async)]
        //public async Task LeaveCmd()
        //{
        //    if (ToricchiStatus.AudioClient != null)
        //    {
        //        await ReplyAsync($"…チッ、そうかい。あばよ。");
        //        await ToricchiStatus.AudioClient.StopAsync();
        //        ToricchiStatus.AudioClient = null;
        //        await ToricchiStatus.VoiceChannel.DeleteAsync();
        //        ToricchiStatus.VoiceChannel = null;
        //    }
        //}
        #endregion

        #region help
        /// <summary>
        /// [help]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("help")]
        public async Task Help()
        {
            // 好感度
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);

            if(commandList == null)
            {
                await ReplyAsync("Initializeメソッドが実行されていないぞ……。");
            }
            string Messages = "これが俺の持っている技の一覧だ。\n```";
            foreach (var a in commandList)
            {
                Messages += a.Key + " : " + a.Value + "\n";
            }
            Messages += "```";
            await ReplyAsync(Messages);
        }
        #endregion
        
        #region IsLike:好感度判定
        /// <summary>
        /// 好感度が一定以上あるか判定する
        /// </summary>
        /// <param name="name"></param>
        /// <param name="like"></param>
        /// <returns></returns>
        private bool IsLike(string name, int like)
        {
            return ToricchiStatus.appData.Characters.Keys.Contains(name) && ToricchiStatus.appData.Characters[name].Like > like;
        }
        #endregion

        /// <summary>
        /// 持ち物リスト表示
        /// </summary>
        /// <returns>持ち物リスト</returns>
        private string ShowInventory()
        {
            var list = ToricchiStatus.appData.Facilities.Values.Where(f => f.Level > 0);

            StringBuilder sb = new StringBuilder();
            sb.Append("```");
            if (list.Count() == 0)
            {
                sb.Append($"\n何もないぜ");
            }
            //sb.Append($"【施設：{ToricchiStatus.Name}の収入を増やします。】");
            foreach (var item in list)
            {
                sb.Append($"\n{item.Name}:レベル{item.Level} 総合生産量{item.CurrentIncome}円\n{item.Comment}");
            }
            //sb.Append("【商品：様々な効果があります。】");
            sb.Append("```");
            return sb.ToString();
        }

        /// <summary>
        /// 買い物リスト表示
        /// </summary>
        /// <param name="budget"></param>
        /// <returns>買い物リスト</returns>
        private string ShowBuyList(int budget)
        {
            var list = ToricchiStatus.appData.Facilities.Values.Where(f => f.CurrentPrice < budget);

            StringBuilder sb = new StringBuilder();
            sb.Append("```");
            if (list.Count() == 0)
            {
                sb.Append($"\n何も買えねーじゃねぇか");
            }
            //sb.Append($"【施設：{ToricchiStatus.Name}の収入を増やします。】");
            foreach (var item in list)
            {
                sb.Append($"\n【{item.Id}】{item.Name}:レベル{item.Level} 生産量{item.BaseIncome}円 価格{item.CurrentPrice}円\n     {item.Comment}");
            }
            //sb.Append("【商品：様々な効果があります。】");
            sb.Append("```");
            return sb.ToString();
        }

        /// <summary>
        /// [!toricchiItemSuccess]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("!toricchiItemSuccess")]
        public async Task ToricchiItemSuccess()
        {
            string Messages = "買ったぜ。ふん、まあこんなもんだろ。";

            if (IsLike(SocketUserMessage.Author.Username, 5))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "良いモン選んでくれたじゃねぇか。大切にするぜ。";
                }
                else
                {
                    Messages = "買ったぜ。お前にしてはまあまあなチョイスだな。";
                }
            }
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [!toricchiItemFailure]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("!toricchiItemFailure")]
        public async Task ToricchiItemFailure()
        {
            string Messages = "テメェわざわざ俺のこと呼んでおいて買い物行かねぇのかよふざけんな！:rage:";

            if (IsLike(SocketUserMessage.Author.Username, 5))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "…何だ何も買わねぇのか。俺は帰るぜ。";
                }
                else
                {
                    Messages = "は？んなもん売ってねぇよ。もういい、帰る。:angry:";
                }
            }
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, -1);

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [inventory]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("inventory")]
        public async Task Inventory()
        {
            string Messages = "テメェ俺のこと嗅ぎまわりやがってどういうつもりだ:rage:";

            if (IsLike(SocketUserMessage.Author.Username, 5))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "ヘヘヘ、いいぜ…。俺のとっておきを見せてやる。:laughing: \n";
                }
                else
                {
                    Messages = "ふん。ちょっと見るだけだからな。:confused:\n";
                }
                // 持ち物リスト表示
                Messages = Messages + ShowInventory();
            }

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [buyitem]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("buyitem")]
        public async Task BuyItem()
        {
            string Messages = "な、何でお前と買い物しなきゃならねぇんだよ……:angry:";
            int budget = ToricchiStatus.Money;
            if (IsLike(SocketUserMessage.Author.Username, 5))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "おう行こうぜ！\n何でも好きなもの買ってやるからな:kissing_heart:\n";
                }
                else
                {
                    Messages = "なんだ買い物か。\n…あまり高いものは買わんぞ。:triumph:\n";
                    budget = budget * 7 / 10;
                }
            }
            else
            {
                // 所持金の3割
                budget = budget * 3 / 10;
            }
            // 一緒に買い物中
            ToricchiStatus.BuyWith = SocketUserMessage.Author.Username;
            ToricchiStatus.budget = budget;
            // 品物リスト表示
            Messages = Messages + ShowBuyList(budget);
            Messages = Messages + "\n何を買えばいいんだ？番号で言ってくれよな。";

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [ステータス]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("ステータス")]
        public async Task Status()
        {
            string Messages = "テメェなんざに教えてやることなんか何もねぇよ！:rage:";
            if (IsLike(SocketUserMessage.Author.Username, 5))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "は、恥ずかしいな…。\nでもお前の頼みだったら……いいぜ//////\n\n```" + ToricchiStatus.ShowStatusAdvance() + "```";
                }
                else
                {
                    Messages = "チッ、俺に興味があるのか。\nちょっとだけだぞ。\n\n```" + ToricchiStatus.ShowStatus() + "```";
                }
                // 好感度
                ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);
            }

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [テスト]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("テスト")]
        public async Task Test()
        {
            string Messages = "試験開始の合図があるまで この問題冊子の中を見ることは許さん:rage:";

            // 好感度
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [あんあんあん]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("あんあんあん")]
        public async Task Ananan()
        {
            string Messages = "おい、汚ねえもん見せつけんな向こうでやれ:angry:" + "\n";

            if (IsLike(SocketUserMessage.Author.Username, 5))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "とってもだいすき:heart_eyes:" + "\n";
                }
                else
                {
                    Messages = "とってもだいすき:blush:" + "\n";
                }
                // 好感度
                ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);
            }

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [お前を消す方法]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("お前を消す方法")]
        public async Task Kyle()
        {
            // 好感度
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, -1);

            string Messages = "それはカイルの兄貴もよく言われたと言ってたぞ……！:cry:";

            if (IsLike(SocketUserMessage.Author.Username, 5))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "そんな悲しいこと言うなよ…。" + "\n";
                }
                else
                {
                    Messages = "俺がお前に消されることはない！" + "\n俺が相当油断していない限りな。";
                }
                // 好感度
                ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, -1);
            }

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [shoottori]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("shoottori")]
        public async Task Shoottori()
        {
            string Messages = "(´・ω);y==ｰｰｰｰｰ  ・ ・ ・  :penguin:   ・∵. ﾀｰﾝ <:sushi:418038060110970880> ＜ｷﾞﾝｷﾞﾝｶﾞｰﾄﾞ\n```ぎんぺーに 5 ダメージを与えた！\nぎんぺーはGOXしました。```\n";

            if (IsLike(SocketUserMessage.Author.Username, 10))
            {
                var randomInt = random.Next() % 2;
                switch (randomInt)
                {
                    case 0:
                        Messages = $"(´・ω);y==ｰｰｰｰｰ  ・ ・   <:sushi:418038060110970880>    ・∵. ﾀｰﾝ\n```{ToricchiStatus.Name}に 10 ダメージを与えた！```\n";
                        Messages = Messages + "ぐっ…油断したぜ。\nお前のことを信じてしまったばかりに……。:confounded:";
                        ToricchiStatus.Hp = Math.Max(0, ToricchiStatus.Hp - 10);
                        break;
                    default:
                        Messages = $"…ん？どうした{SocketUserMessage.Author.Username}？";
                        Messages = Messages + $"```{SocketUserMessage.Author.Username}は、{ToricchiStatus.Name}の背後から優しく抱きしめて引き金を引いた。```\n";
                        Messages = Messages + $"(´・ω);y==<:sushi:418038060110970880>    ・∵. ﾀｰﾝ\n```{ToricchiStatus.Name}に 20 ダメージを与えた！```\n";
                        Messages = Messages + "ぐあああああああっ！！！！:tired_face:";
                        Messages = Messages + $"```{SocketUserMessage.Author.Username}は、{ToricchiStatus.Name}の血を腹に塗りながら笑った。```\n";
                        ToricchiStatus.Hp = Math.Max(0, ToricchiStatus.Hp - 20);
                        break;
                }

                // 好感度
                ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, -2);
            }
            else
            {
                var randomInt = random.Next() % 2;
                switch (randomInt)
                {
                    case 0:
                        Messages = Messages + "ふん、のろまなお前に俺が撃てるわけねぇだろ:smirk:";
                        break;
                    default:
                        Messages = Messages + "…おいお前何てことしやがるんだ！\n見損なったぞ！:rage:";
                        break;
                }
                // 好感度
                ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, -1);
            }

            // 死亡判定
            if(ToricchiStatus.Hp <= 0)
            {
                Messages = Messages + $"\n```{SocketUserMessage.Author.Username}は{ToricchiStatus.Name}を倒した！```\n";
                Messages = Messages + $"………ッ！\nゴホッ……かはっ……！\n強くなったな{SocketUserMessage.Author.Username}…。だが、覚えているがいい。\nお前の心に欲望がある限り、俺は何度でも蘇るだろう。\nその時までせいぜいつかの間の平和を楽しむがいい……。";
                ToricchiStatus.ResetLike(SocketUserMessage.Author.Username);
                ToricchiStatus.isDead = true;
                Messages = Messages + $"\n```こうして、このチャンネルに再び平和が訪れました。\nめでたしめでたし```";
                Messages = Messages + $"\n```とりっっち　制作スタッフ\n\n企画　ぎんぺー\n原案　ぎんぺー\n設計　ぎんぺー\nメインプログラム　ぎんぺー\nシナリオ　ぎんぺー\n疲労　ぎんぺー\n\n\nAND YOU\n\n\n\nおしまい```";
            }

            await ReplyAsync(Messages); 
        }

        /// <summary>
        /// [おにいちゃん]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("おにいちゃん")]
        public async Task Brother()
        {
            string Messages = "…あぁ？:anguished:";

            if (IsLike(SocketUserMessage.Author.Username, 3))
            {
                if (IsLike(SocketUserMessage.Author.Username, 10))
                {
                    Messages = "どうした" + SocketUserMessage.Author.Username + "、俺が恋しくなったのか？\n可愛いやつめ……。";
                }
                else
                {
                    Messages = "なんだよ？何か悩みでもあるのか？\nか、勘違いするなよ！お前に兄貴呼ばわりされるのがうっとうしいだけだ。\nべ、別に心配なんかしてねぇんだからなっ:angry:";
                }
            }

            // 好感度
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);

            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [デジタルメガフレア]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("デジタルメガフレア")]
        public async Task DigitalMegaFlare()
        {
            string Messages = "";
            if (ToricchiStatus.Mp >= 100)
            {
                ToricchiStatus.Mp -= 100;
                Messages = $"「天よ地よ大いなる神よ\n　生きとし生けるもの皆終焉の雄叫びを上げ\n　舞い狂う死神達の宴を始めよ\n　冥界より召喚されし暗黒の扉今開かれん\n　*デジタルメガフレアーーーーーッ！！*」\n\n{ToricchiStatus.Name}の指先から熱線が放たれ、Zaifに深刻なダメージを与えた！！\nZaifはGOXしました。\n{ToricchiStatus.Name}は{ToricchiStatus.Income * 1000}円獲得しました。";
                ToricchiStatus.Money += ToricchiStatus.Income * 1000;
            }
            else
            {
                Messages = "す、すまん。MPが足りないようだ……。";
            }
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);
            
            await ReplyAsync(Messages);
        }

        #region unko
        /// <summary>
        /// [うんこ]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("うんこ")]
        public async Task Unko()
        {
            if (ToricchiStatus.Unko >= 50)
            {
                // 好感度
                ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, -1);
                ToricchiStatus.Unko = 0;

                string Messages = "うんこ食ってるときにカレーの話をしてんじゃねぇ！:rage:";
                await ReplyAsync(Messages);
            }
        }

        /// <summary>
        /// [下痢]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("下痢")]
        public async Task Geri()
        {
            await Unko();
        }
        #endregion

        #region おなかすいた
        /// <summary>
        /// [お腹空いた]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("お腹空いた")]
        public async Task Onaka()
        {
            string Messages = "ピーナッツでも食ってろ、剥いてやっからよ。";

            if (IsLike(SocketUserMessage.Author.Username, 3))
            {
                if (IsLike(SocketUserMessage.Author.Username, 6))
                {
                    Messages = "そうか、" + SocketUserMessage.Author.Username + "は腹が減っているのか\n…俺のチョコソフトクリームやるよ。";
                }
                else
                {
                    Messages = "俺のカレーせんべい食えよ。";
                }
            }
            // 好感度
            ToricchiStatus.CulcLike(SocketUserMessage.Author.Username, 1);
            await ReplyAsync(Messages);
        }

        /// <summary>
        /// [おなかすいた]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("おなかすいた")]
        public async Task Onaka2()
        {
            await Onaka();
        }

        /// <summary>
        /// [腹減った]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("腹減った")]
        public async Task Onaka3()
        {
            await Onaka();
        }

        /// <summary>
        /// [はらへった]というコメントが来た際の処理
        /// </summary>
        /// <returns>Botのコメント</returns>
        [Command("はらへった")]
        public async Task Onaka4()
        {
            await Onaka();
        }
        #endregion


    }
}
