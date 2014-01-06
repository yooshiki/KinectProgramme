using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyKinectTool_
{
    //敵へのダメージ
    //
    class enemy
    {
        private static float damage1=10.0f;
        private static float damage2 = 100.0f;

        //敵が攻撃1を食らった場合
        public static float Edamage1()
        {
            return damage1;
        }

        //敵が攻撃Ⅱを食らった場合
        public static float Edamage2()
        {
            return damage2;
        }
    }
}
