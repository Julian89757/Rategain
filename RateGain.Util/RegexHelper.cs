using System.Text.RegularExpressions;

namespace RateGain.Util
{
    /// <summary>
    /// 表示正则表达式帮助类
    /// </summary>
    public static class RegexHelper
    {
        /// <summary>
        /// 手机号码正则表达式
        /// </summary>
        public static readonly Regex RegMobile = new Regex(@"^(1[0-9])\d{9}$");

        /// <summary>
        /// 验证手机号码格式是否正确
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool IsMobile(this string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
                return false;
            return RegMobile.IsMatch(mobile);
        }

        /// <summary>
        /// 邮箱正则表达式
        /// </summary>
        public static readonly Regex RegEmail = new Regex(@"[\w!#$%&'*+/=?^_`{|}~-]+(?:\.[\w!#$%&'*+/=?^_`{|}~-]+)*@[a-zA-Z0-9]{1,}(\-)?[a-zA-Z0-9]{0,}(\.)[a-zA-Z]{2,}");

        /// <summary>
        /// 验证邮箱格式是否正确
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return RegEmail.IsMatch(email);
        }

        /// <summary>
        /// 密码强度正则表达式_强
        /// </summary>
        public static readonly Regex RegPasswordStrong = new Regex(@"^(?![a-zA-z]+$)(?!\d+$)(?![!@#$%^&*]+$)(?![a-zA-z\d]+$)(?![a-zA-z!@#$%^&*]+$)(?![\d!@#$%^&*]+$)[a-zA-Z\d!@#$%^&*]+$");
        /// <summary>
        /// 密码强度正则表达式_中
        /// </summary>
        public static readonly Regex RegPasswordAverage = new Regex(@"^(?![a-zA-z]+$)(?!\d+$)(?![!@#$%^&*]+$)[a-zA-Z\d!@#$%^&*]+$");
        /// <summary>
        /// 密码强度正则表达式_弱
        /// </summary>
        public static readonly Regex RegPasswordWeak = new Regex(@"^(?:\d+|[a-zA-Z]+|[!@#$%^&*]+)$");
        /// <summary>
        /// 密码强度正则表达式_长度
        /// </summary>
        public static readonly Regex RegPasswordLength = new Regex(@"^[\S]{6,20}$");

        /// <summary>
        /// 根据正则表达式计算密码长度是否合格
        /// </summary>
        /// <param name="password"></param>
        /// <returns>true/false</returns>
        public static bool PasswordLength(this string password)
        {
            if (string.IsNullOrEmpty(password) || !RegPasswordLength.IsMatch(password))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据正则表达式计算密码强度
        /// </summary>
        /// <param name="password"></param>
        /// <returns>返回密码强度等级(0=不合格,1=弱,2=中,3=强)</returns>
        public static int PasswordIntensity(this string password)
        {
            if (string.IsNullOrEmpty(password) || !RegPasswordLength.IsMatch(password))
            {
                return 0;
            }
            if (RegPasswordWeak.IsMatch(password))
            {
                return 1;
            }
            if (RegPasswordAverage.IsMatch(password))
            {
                return 2;
            }
            if (RegPasswordStrong.IsMatch(password))
            {
                return 3;
            }
            return 0;
        }


        /// <summary>
        /// 字符串自定义正则验证
        /// </summary>
        /// <param name="str"></param>
        /// <param name="customerRegex"></param>
        /// <returns></returns>
        public static bool IsCustomerRegex(this string str, Regex customerRegex)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return customerRegex.IsMatch(str);
        }
    }
}
