using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace blqw.Unitable
{
    /// <summary>
    /// 拓展方法
    /// </summary>
    internal static class ExtensionsMethods
    {
        /// <summary>
        /// 安全的获取程序集中的所有类型
        /// </summary>
        public static IEnumerable<TypeInfo> SafeGetTypes(this Assembly assembly)
            => SafeGetTypes(assembly, out _);

        /// <summary>
        /// 安全的获取程序集中的所有类型
        /// </summary>
        public static IEnumerable<TypeInfo> SafeGetTypes(this Assembly assembly, out ReflectionTypeLoadException ex)
        {
            try
            {
                ex = null;
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException e)
            {
                ex = e;
                return e.Types.Where(x => x != null).Select(IntrospectionExtensions.GetTypeInfo);
            }
        }

        readonly static char[] _dot = new[] { '.' };

        /// <summary>
        /// 验证元件名称
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static (string name, string groupName) ValidName(this string name)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                return (null, null);
            }
            var arr = name.Split(_dot);
            if (arr.Length == 1)
            {
                return name.All(vaild) && !name[0].InRange('0', '9') ? (name, (string)null) : (null, null);
            }
            if (arr.Length == 2 && arr[1].Length > 0)
            {
                return arr[0].All(vaild) && arr[1].All(vaild) && !name[0].InRange('0', '9') ? (name, arr[0]) : (null, null);
            }
            return (null, null);
            bool vaild(char c) => c.InRange('0', '9') || c.InRange('a', 'z') || c.InRange('A', 'Z');
        }

        /// <summary>
        /// 比较一个值是否在某个限定的范围
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange<T>(this T value, T min, T max)
            where T : IComparable
            => min.CompareTo(value) <= 0 && max.CompareTo(value) >= 0;

        /// <summary>
        /// 比较一个值是否在某个限定的范围
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange<T>(this T value, IComparable<T> min, IComparable<T> max)
            => min.CompareTo(value) <= 0 && max.CompareTo(value) >= 0;
        /// <summary>
        /// 调用Disposable方法,无论如果都不抛出异常
        /// </summary>
        /// <param name="disposable"></param>
        public static void SafeDispose(this IDisposable disposable)
        {
            if (disposable == null)
            {
                return;
            }

            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error($"释放对象:{disposable.GetType().FullName} 出现异常", ex);
            }
        }
    }
}
