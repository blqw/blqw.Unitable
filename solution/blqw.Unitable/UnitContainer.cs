using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace blqw.Unitable
{
    /// <summary>
    /// 元件容器
    /// </summary>
    public class UnitContainer : IServiceProvider
    {
        /// <summary>
        /// 全局容器
        /// </summary>
        public static UnitContainer Global { get; } = new UnitContainer(() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(ExtensionsMethods.SafeGetTypes));

        public UnitContainer(Func<IEnumerable<TypeInfo>> exportedTypes)
        {
            if (exportedTypes == null)
            {
                throw new ArgumentNullException(nameof(exportedTypes));
            }


        }


        private ConcurrentDictionary<string, NamedUnit> _allUints = new ConcurrentDictionary<string, NamedUnit>();

        private ConcurrentDictionary<string, NamedUnit> _groupUints = new ConcurrentDictionary<string, NamedUnit>();


        public object GetService(Type serviceType) => throw new NotImplementedException();
    }
}
