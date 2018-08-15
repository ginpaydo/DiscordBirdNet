using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscordBirdNet.Model;

namespace DiscordBirdNet
{
    /// <summary>
    /// メモリ上のデータをここに集約
    /// </summary>
    public class AppData
    {
        /// <summary>
        /// 利用者
        /// </summary>
        public Dictionary<string, Character> Characters { get; set; }

        /// <summary>
        /// とりっっちのパラメータ
        /// </summary>
        public Dictionary<string, Parameter> Parameters { get; set; }

        /// <summary>
        /// 施設一覧
        /// </summary>
        public Dictionary<string, Facility> Facilities { get; set; }

        /// <summary>
        /// パラメータが存在していなければ初期化する
        /// </summary>
        /// <param name="parameterName">パラメータの名前</param>
        /// <param name="value">初期値</param>
        public void InitializeParamater(string parameterName, string value)
        {
            if (!Parameters.ContainsKey(parameterName))
            {
                Parameters.Add(parameterName, new Parameter() { Name = parameterName, Value = value });
            }
        }

        /// <summary>
        /// 施設データの初期化
        /// なかったら追加する
        /// </summary>
        public void InitializeFacility(string facilityName, int basePrice, int baseIncome, string comment)
        {
            if (!Facilities.ContainsKey(facilityName))
            {
                Facilities.Add(facilityName, new Facility() { Name = facilityName, BasePrice = basePrice, BaseIncome = baseIncome, Level = 0, CurrentPrice = basePrice, CurrentIncome = 0, Comment = comment });
            }
        }
    }
}
