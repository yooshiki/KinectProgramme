using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class FileSave
    {
        public void FileWrite(bool B)
        {
            StreamWriter stw = new StreamWriter("data.txt");
            DateTime dt = DateTime.Now;
            if (B)
            {
                stw.WriteLine("{0}H{1}M{2}S : " + "true", dt.Hour, dt.Minute, dt.Second);

            }
            else
            {
                stw.WriteLine("{0}H{1}M{2}S : " + "false", dt.Hour, dt.Minute, dt.Second);
            }
        }
    }
}
