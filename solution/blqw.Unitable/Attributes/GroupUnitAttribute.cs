using System;
using System.Collections.Generic;
using System.Text;

namespace blqw.Unitable.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class GroupUnitAttribute : Attribute
    {
        public GroupUnitAttribute(string name, string groupName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            Name = name;
            GroupName = groupName;
        }

        /// <summary>
        /// 元件的名称
        /// </summary>
        public string Name { get; }
        public string GroupName { get; }

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
