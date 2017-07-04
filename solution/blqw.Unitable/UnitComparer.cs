using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace blqw.Unitable
{
    internal sealed class UnitComparer : IComparer<IUnit>
    {
        public static UnitComparer Default { get; } = new UnitComparer(true, false);
        public static UnitComparer WithPriority { get; } = new UnitComparer(true, true);
        internal static UnitComparer NoWithName { get; } = new UnitComparer(false, false);

        private readonly bool _withName;
        private readonly bool _withPriority;

        public UnitComparer(bool withName, bool withPriority)
        {
            _withName = withName;
            _withPriority = withPriority;
        }

        public int Compare(IUnit x, IUnit y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }
            if (x == null)
            {
                return 1;
            }
            if (y == null)
            {
                return -1;
            }

            if (_withName)
            {
                var c = StringComparer.Ordinal.Compare(x.Name, y.Name);
                if (c != 0)
                {
                    return c;
                }
            }
            if (_withPriority == false || x.Verion != y.Verion)
            {
                return y.Verion.CompareTo(x.Verion);
            }
            return y.Priority.CompareTo(x.Priority);
        }
    }
}
