// ファイル保存のクラス
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Threading; // Timer
using Microsoft.Win32; // for file
using System.Windows.Media.Imaging;
using System.Windows;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class FileSave
    {
        SaveFileDialog sfd = new SaveFileDialog(); // インスタンスを生成
        string Pass;
        //public void SaveFile(bool flg1, bool flg2, bool flg3, bool flg4, bool flg5, bool flg6)
        public string SaveFile(int timecount, string pass, bool flg, bool flg2, bool flg3, bool flg4, bool flg5, bool flg6)
        {
            // .csv でファイルを出力する

            //StreamWriter sw = new StreamWriter("data.csv", true);
            // 経過時間が60秒・スタート時またはパスがからであればダイアログを表示する
            if (timecount % 60 == 0 || Pass == null)
            {
                bool? result = sfd.ShowDialog();
                sfd.Filter = "CSVファイル(*.csv)|*.csv";
                sfd.Title = "CSVファイルの書き出し";

                if (result == true)
                {
                    Pass = sfd.FileName;

                    DateTime dt = DateTime.Now;
                    StreamWriter sw = new StreamWriter(@Pass, true);    // 指定したパスを元にファイルを上書き保存する

                    sw.Write(dt.ToLongTimeString() + ",");  // 保存時間の記録
                    sw.Write(flg.ToString() + "," + flg2.ToString() + "," + flg3.ToString() + "," +
                        flg4.ToString() + "," + flg5.ToString() + "," + flg6.ToString() + "," +
                        ParsonCount(flg, flg2, flg3, flg4, flg5, flg6));

                    sw.WriteLine(); // 改行する
                    sw.Close();     // ファイルを閉じる
                }
            }
            else
            {
                Pass = pass; // ファイル名を受け渡す。

                // 記録時間に扱う
                DateTime dt = DateTime.Now;
                //FileStream fileStream = (FileStream)sfd.OpenFile();
                StreamWriter sw = new StreamWriter(@Pass, true);    // 指定したパスを元にファイルを上書き保存する
                // 表にするため縦と横を用意する
                //for (int i = 0; i < flg1.Length; i++)
                //{
                // CSVファイルにデータを記述する
                sw.Write(dt.ToLongTimeString() + ",");  // 保存時間の記録
                sw.Write(flg.ToString() + "," + flg2.ToString() + "," + flg3.ToString() + "," +
                    flg4.ToString() + "," + flg5.ToString() + "," + flg6.ToString() + "," +
                    ParsonCount(flg, flg2, flg3, flg4, flg5, flg6));
                //sw.Write(flg1.ToString() + "," + flg2.ToString() + "," + flg3.ToString() + "," + flg4.ToString() + "," + 
                //    flg5.ToString() + "," + flg6.ToString());
                //for (int j = 0; j < 6; j++)
                //{
                //    sw.Write(flg1[i].ToString() + ",");     // 真偽を文字列で保存
                //}

                sw.WriteLine();                         // 改行する
                sw.Close(); // ファイルを閉じる
                //}
            }

            return Pass; // ファイルの名前を返す。
        }

        // 記録終了時に実行されるメソッド（人数の合計を算出し記録する）
        public void Finish(string pass, int[] count)
        {
            StreamWriter sw = new StreamWriter(@pass, true, Encoding.GetEncoding("shift-jis"));
            int total = 0;

            for (int i = 0; i < count.Length; i++)
            {
                total = total + count[i];
            }

            sw.Write("合計：" + ",");
            sw.Write("{0}" + ",",total);
            sw.WriteLine("人");

            sw.Close();
        }

        // 配列から認識した人数をカウントするメソッド
        public int ParsonCount(bool flg, bool flg2, bool flg3, bool flg4, bool flg5, bool flg6)
        {
            int total = 0;
            bool[] array = new bool[6];

            array[0] = flg;
            array[1] = flg2;
            array[2] = flg3;
            array[3] = flg4;
            array[4] = flg5;
            array[5] = flg6;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == true)
                {
                    total++;
                }
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = false;
            }

            return total; // 合計を返す
        }

        // スクリーンショットを保存するメソッド
        public void PictSave(BitmapEncoder encoder, string path, StreamWriter sw)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            if (path == "")
            {
                sfd.ShowDialog();
                Pass = sfd.FileName;
                try
                {
                    using (FileStream fs = new FileStream(@Pass, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    MessageBox.Show("以下の場所へ画像を保存しました。\n" + @Pass,
                        "ファイルの保存に成功しました！", MessageBoxButton.OK, MessageBoxImage.Information);
                    sw.WriteLine(sfd.FileName);
                    sw.Close();
                    //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
                }
                catch (IOException)
                {
                    //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
                    MessageBox.Show("画像の保存に失敗しました。", "画像の保存エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    //sfd.Filter = "PNGファイル(*.png)";
                    //sfd.Title = "スクリーンショットの保存";
                    sw.WriteLine(sfd.SafeFileName);
                    sw.Close();
                }
            }
            else
            {

                try
                {
                    using (FileStream fs = new FileStream(@path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    MessageBox.Show("以下の場所へ画像を保存しました。\n" + @path,
                        "ファイルの保存に成功しました！", MessageBoxButton.OK, MessageBoxImage.Information);
                    sw.WriteLine(Pass);
                    sw.Close();
                    //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
                }
                catch (IOException)
                {
                    //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
                    MessageBox.Show("画像の保存に失敗しました。", "画像の保存エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
