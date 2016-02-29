using System;
using System.Text;

namespace RateGain.Util
{
    /// <summary>
    /// 表示Random生成伪随机数类
    /// </summary>
    public class RandomHelper
    {
        //随机数对象
        private readonly Random _random;
 
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public RandomHelper()
        {
            //为随机数对象赋值
            _random = new Random();
        }
        #endregion
 
        #region 生成一个指定范围的随机整数
        /// <summary>
        /// 生成一个指定范围的随机整数，该随机数范围包括最小值，但不包括最大值
        /// </summary>
        /// <param name="minNum">最小值</param>
        /// <param name="maxNum">最大值</param>
        public int GetRandomInt(int minNum, int maxNum)
        {
            return _random.Next(minNum, maxNum);
        }
        #endregion
 
        #region 生成一个0.0到1.0的随机小数
        /// <summary>
        /// 生成一个0.0到1.0的随机小数
        /// </summary>
        public double GetRandomDouble()
        {
            return _random.NextDouble();
        }
        #endregion
 
        #region 对一个数组进行随机排序
        /// <summary>
        /// 对一个数组进行随机排序
        /// </summary>
        /// <typeparam name="T">数组的类型</typeparam>
        /// <param name="arr">需要随机排序的数组</param>
        public void GetRandomArray<T>(T[] arr)
        {
            //对数组进行随机排序的算法:随机选择两个位置，将两个位置上的值交换
 
            //交换的次数,这里使用数组的长度作为交换次数
            var count = arr.Length;
 
            //开始交换
            for (var i = 0; i < count; i++)
            {
                //生成两个随机数位置
                var randomNum1 = GetRandomInt(0, arr.Length);
                var randomNum2 = GetRandomInt(0, arr.Length);
 
                //定义临时变量

                //交换两个随机数位置的值
                var temp = arr[randomNum1];
                arr[randomNum1] = arr[randomNum2];
                arr[randomNum2] = temp;
            }
        }
 
 
        // 一：随机生成不重复数字字符串 
        private int _rep;
        public string GenerateCheckCodeNum(int codeCount)
        {
            var str = string.Empty;
            var num2 = DateTime.Now.Ticks + _rep;
            _rep++;
            var random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> _rep)));
            for (var i = 0; i < codeCount; i++)
            {
                var num = random.Next();
                str = str + ((char)(0x30 + ((ushort)(num % 10))));
            }
            return str;
        }
 
        //方法二：随机生成字符串（数字和字母混和）
        public string GenerateCheckCode(int codeCount)
        {
            var str = string.Empty;
            var num2 = DateTime.Now.Ticks + _rep;
            _rep++;
            var random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> _rep)));
            for (var i = 0; i < codeCount; i++)
            {
                char ch;
                var num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch;
            }
            return str;
        }
 
        #region
 
        /// <summary>
        /// 从字符串里随机得到，规定个数的字符串.
        /// </summary>
        /// <param name="allChar"></param>
        /// <param name="codeCount"></param>
        /// <returns></returns>
        public string GetRandomCode(string allChar, int codeCount)
        {
            //string allChar = "1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,i,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            var allCharArray = allChar.Split(',');
            var randomCode = "";
            var temp = -1;
            var rand = new Random();
            for (var i = 0; i < codeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(temp * i * ((int)DateTime.Now.Ticks));
                }
 
                var t = rand.Next(allCharArray.Length - 1);
 
                while (temp == t)
                {
                    t = rand.Next(allCharArray.Length - 1);
                }
 
                temp = t;
                randomCode += allCharArray[t];
            }
            return randomCode;
        }

        /// <summary>
        /// 从字符串里随机得到，规定个数的字符串.
        /// </summary>
        /// <param name="minCount"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public string GetRandomCode(int minCount, int maxCount)
        {
            var array = new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
                'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z',
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                '-', '+', '_', '$'
            };
            var length = _random.Next(minCount, maxCount);
            var strBuilder = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var index = _random.Next(0, array.Length - 1);
                strBuilder.Append(array[index]);
            }
            return strBuilder.ToString();
        }
 
        #endregion
        #endregion

        public static RandomHelper Current
        {
            get
            {
                return new RandomHelper();
            }
        }
    }
}