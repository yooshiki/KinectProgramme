// ファイル保存のクラス
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Microsoft.Samples.Kinect.ColorBasics
{
    class FileSave
    {
        //public void SaveFile(bool flg1, bool flg2, bool flg3, bool flg4, bool flg5, bool flg6)
        public void SaveFile(bool flg, bool flg2, bool flg3, bool flg4, bool flg5, bool flg6)
        {         
            
         
            
            // .csv でファイルを出力する
            StreamWriter sw = new StreamWriter("data.csv", true);
     
            // 記録時間に扱う
            DateTime dt = DateTime.Now;

            // 表にするため縦と横を用意する
            //for (int i = 0; i < flg1.Length; i++)
            //{
                sw.Write(dt.ToLongTimeString() + ",");  // 保存時間の記録
                sw.Write(flg.ToString()+ "," + flg2.ToString()+ "," + flg3.ToString()+ "," +
                    flg4.ToString()+ "," + flg5.ToString()+ "," + flg6.ToString());
                //sw.Write(flg1.ToString() + "," + flg2.ToString() + "," + flg3.ToString() + "," + flg4.ToString() + "," + 
                //    flg5.ToString() + "," + flg6.ToString());
                //for (int j = 0; j < 6; j++)
                //{
                //    sw.Write(flg1[i].ToString() + ",");     // 真偽を文字列で保存
                //}

            //sw.WriteLine();                         // 改行する
            //}
           
            //sw.Close(); // ファイルを閉じる
        }
    }
}
