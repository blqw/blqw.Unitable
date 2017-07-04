using System;
using System.Collections.Generic;
using System.Text;

namespace blqw.Unitable
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class UnitAttribute : Attribute
    {
        public UnitAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
        }

        /// <summary>
        /// 元件的名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 元件的版本
        /// </summary>
        public decimal Verion { get; set; }

        /// <summary>
        /// 元件的优先级
        /// </summary>
        public int Priority { get; set; }
    }
}
