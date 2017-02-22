using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateGain.Util
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// 在IEnumerable<T> 循环或者变为内存序列之前 忽略掉序列中存在的元素异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<T> SkipException<T>(this IEnumerable<T> values)
        {
            using (var enumerator = values.GetEnumerator() )
            {
                var next = true;
                while(next)
                {
                    try
                    {
                        // 如果枚举器成功推进到下一个元素，MoveNext为true；枚举数越过集合结尾，则为false，也就是说返回值只确定后续是否有值
                        next = enumerator.MoveNext();
                    }catch
                    {
                        // catch到异常，忽略当前元素，继续循环
                        LogHelper.Write("cause exception",LogHelper.LogMessageType.Error );
                        continue;
                    }
                    // yield 返回枚举器指向的当前元素
                    if (next)
                        yield return enumerator.Current;
                }
            }
        }
    }
}
