using System;

namespace blqw.Unitable
{
    /// <summary>
    /// 表示一个元件
    /// </summary>
    public interface IUnit: IDisposable
    {
        /// <summary>
        /// 元件的名称, [组名.]插件名, 只允许包含字母和数字, 不允许数字开头
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 元件组的名称, 没有组别的元件, 该值为null
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// 元件的版本
        /// </summary>
        decimal Verion { get; }

        /// <summary>
        /// 元件的优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 将元件尝试转为指定的类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        (bool success, object value) TryConvert(Type type);
    }
}
