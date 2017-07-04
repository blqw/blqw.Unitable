using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace blqw.Unitable
{
    /// <summary>
    /// 元件
    /// </summary>
    public sealed class NamedUnit : IUnit, IDisposable, IObservable<(UnitEventType, IUnit)>, IReadOnlyDictionary<decimal, IUnit>
    {
        /// <summary>
        /// 初始化元件集合
        /// </summary>
        /// <param name="name"></param>
        public NamedUnit(string name)
        {
            (Name, GroupName) = name.ValidName();
            if (Name == null)
            {
                throw new ArgumentException("元件名称不合法", nameof(name));
            }
            _locker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 元件名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 元件的组成名
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// 当前集合中版本最高的元件
        /// </summary>
        public IUnit DefaultUnit => _defaultUnit;

        /// <summary>
        /// 读写锁
        /// </summary>
        private readonly ReaderWriterLockSlim _locker;

        /// <summary>
        /// 元件列表
        /// </summary>
        private List<IUnit> _units;

        /// <summary>
        /// 默认元件
        /// </summary>
        /// <remarks>只有一个,则不实例化 _units, 以节省资源</remarks>
        private IUnit _defaultUnit;

        /// <summary>
        /// 获取指定版本的元件, 如果不存在返回null
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public IUnit this[decimal version]
        {
            get
            {
                if (_units == null)
                {
                    return _defaultUnit?.Verion == version ? _defaultUnit : null;
                }
                try
                {
                    _locker.EnterReadLock();
                    var index = BinarySearch(version);
                    return index >= 0 ? _units[index] : null;
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 获取指定范围版本中最高版本的元件
        /// </summary>
        /// <param name="minVersion"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        public IUnit this[decimal minVersion, decimal maxVersion]
        {
            get
            {
                if (_units == null)
                {
                    return (_defaultUnit?.Verion.InRange(minVersion, maxVersion) ?? false) ? _defaultUnit : null;
                }
                try
                {
                    _locker.EnterReadLock();
                    var index = BinarySearch(maxVersion);
                    if (index >= 0)
                    {
                        return _units[index];
                    }
                    index = ~index;
                    if (index >= _units.Count)
                    {
                        return null;
                    }
                    var next = _units[index];
                    return next.Verion >= minVersion ? next : null;
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        private int BinarySearch(decimal version)
        {
            var begin = 0;
            var end = _units.Count - 1;
            while (begin <= end)
            {
                var i = begin + ((end - begin) >> 1);
                var order = decimal.Compare(_units[i].Verion, version);

                if (order == 0)
                {
                    return i;
                }
                if (order > 0)
                {
                    begin = i + 1;
                }
                else
                {
                    end = i - 1;
                }
            }

            return ~begin;
        }

        /// <summary>
        /// 添加元件到集合
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool Add(IUnit unit)
        {
            if (_isdisposed)
            {
                throw new ObjectDisposedException($"{nameof(NamedUnit)}:{Name}");
            }
            if (unit == null || string.Equals(unit.Name, Name, StringComparison.Ordinal) == false
                || string.Equals(unit.GroupName, GroupName, StringComparison.Ordinal) == false)
            {
                return false;
            }
            if (_defaultUnit == null)
            {
                Interlocked.Exchange(ref _defaultUnit, unit);
                PushNext(UnitEventType.Add, unit);
                return true;
            }
            try
            {
                _locker.EnterWriteLock();
                if (_units == null)
                {
                    _units = new List<IUnit>() { _defaultUnit };
                }

                var index = BinarySearch(unit.Verion);
                if (index < 0)
                {
                    if (~index == 0)
                    {
                        Interlocked.Exchange(ref _defaultUnit, unit);
                        _units.Insert(0, unit);
                    }
                    else
                    {
                        _units.Insert(~index, unit);
                    }
                    PushNext(UnitEventType.Add, unit);
                    return true;
                }
                else
                {
                    var old = _units[index];
                    if (old.Priority <= unit.Priority)
                    {
                        if (index == 0)
                        {
                            Interlocked.Exchange(ref _defaultUnit, unit);
                        }
                        old.Dispose();
                        PushNext(UnitEventType.Remove, old);
                        _units[index] = unit;
                        PushNext(UnitEventType.Replace, unit);
                        return true;
                    }
                }
                return false;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 最高版本
        /// </summary>
        public decimal Verion => _defaultUnit?.Verion ?? 0;

        /// <summary>
        /// 最高版本元件的优先级
        /// </summary>
        public int Priority => _defaultUnit?.Priority ?? -1;

        /// <summary>
        /// 使用最高版本转换
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public (bool success, object value) TryConvert(Type type) => _defaultUnit?.TryConvert(type) ?? (false, null);


        public bool ContainsKey(decimal version)
        {
            try
            {
                _locker.EnterReadLock();
                return BinarySearch(version) >= 0;
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        public bool TryGetValue(decimal version, out IUnit value) => (value = this[version]) != null;

        IEnumerable<decimal> IReadOnlyDictionary<decimal, IUnit>.Keys
        {
            get
            {
                List<decimal> arr;
                try
                {
                    _locker.EnterReadLock();
                    arr = _units.ConvertAll(x => x.Verion);
                }
                finally
                {
                    _locker.ExitReadLock();
                }
                return arr;
            }
        }

        IEnumerable<IUnit> IReadOnlyDictionary<decimal, IUnit>.Values
        {
            get
            {
                IUnit[] arr;
                try
                {
                    _locker.EnterReadLock();
                    arr = new IUnit[_units.Count];
                    _units.CopyTo(arr);
                }
                finally
                {
                    _locker.ExitReadLock();
                }
                return arr;
            }
        }

        public int Count => _units?.Count ?? (_defaultUnit == null ? 0 : 1);

        public IEnumerator<KeyValuePair<decimal, IUnit>> GetEnumerator()
        {
            List<KeyValuePair<decimal, IUnit>> arr;
            try
            {
                _locker.EnterReadLock();
                arr = _units.ConvertAll(x => new KeyValuePair<decimal, IUnit>(x.Verion, x));
            }
            finally
            {
                _locker.ExitReadLock();
            }
            return arr.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region 观察者

        /// <summary>
        /// 所有已订阅的观察者
        /// </summary>
        private List<IObserver<(UnitEventType, IUnit)>> _observers = new List<IObserver<(UnitEventType, IUnit)>>();
        /// <summary>
        /// 观察者集合的只读副本, 为了防止发送消息时有并发订阅行为
        /// </summary>
        private IObserver<(UnitEventType, IUnit)>[] _observersReadOnlyCopy;
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<(UnitEventType, IUnit)> observer)
        {
            if (_isdisposed)
            {
                throw new ObjectDisposedException($"{nameof(NamedUnit)}:{Name}");
            }
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (!_observers.Contains(observer))
            {
                lock (_observers)
                {
                    if (!_observers.Contains(observer))
                    {
                        _observers.Add(observer);
                        _observersReadOnlyCopy = null;
                    }
                }
            }
            return new Unsubscribe(this, observer);
        }
        /// <summary>
        /// 获取只读的观察者集合
        /// </summary>
        /// <returns></returns>
        private IObserver<(UnitEventType, IUnit)>[] GetReadOnlyObserversCopy()
        {
            return _observersReadOnlyCopy ?? copy();
            IObserver<(UnitEventType, IUnit)>[] copy()
            {
                lock (_observers)
                {
                    if (_observersReadOnlyCopy == null)
                    {
                        _observersReadOnlyCopy = new IObserver<(UnitEventType, IUnit)>[_observers.Count];
                        _observers.CopyTo(_observersReadOnlyCopy, 0);
                    }
                    return _observersReadOnlyCopy;
                }
            }
        }

        private void PushCompleted()
        {
            var observers = GetReadOnlyObserversCopy();
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    Logger.Error("观察者OnCompleted异常", ex);
                }
            }
        }

        private void PushError(Exception error)
        {
            var observers = GetReadOnlyObserversCopy();
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnError(error);
                }
                catch (Exception ex)
                {
                    Logger.Error("观察者OnError异常", ex);
                }
            }
        }

        private void PushNext(UnitEventType type, IUnit unit)
        {
            var observers = GetReadOnlyObserversCopy();
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnNext((type, unit));
                }
                catch (Exception ex)
                {
                    Logger.Error("观察者OnNext异常", ex);
                }
            }
        }

        private class Unsubscribe : IDisposable
        {
            private NamedUnit _server;
            private IObserver<(UnitEventType, IUnit)> _observer;

            public Unsubscribe(NamedUnit server, IObserver<(UnitEventType, IUnit)> observer)
            {
                _server = server;
                _observer = observer;
            }

            public void Dispose()
            {
                var s = _server;
                if (s != null)
                {
                    lock (s)
                    {
                        if (s._observers.Remove(_observer))
                        {
                            s._observersReadOnlyCopy = null;
                        }
                    }
                }
            }
        }

        #endregion

        #region IDisposable Support
        private bool _isdisposed = false; // 要检测冗余调用

        private void Dispose(bool disposing)
        {
            if (!_isdisposed)
            {
                var units = _units;
                if (disposing)
                {
                    _units = null;
                    _defaultUnit = null;
                    PushCompleted();
                    _observers.Clear();
                    _observers = null;
                    _observersReadOnlyCopy = null;
                }
                _locker.SafeDispose();
                units?.ForEach(ExtensionsMethods.SafeDispose);
                _isdisposed = true;
            }
        }


        ~NamedUnit() => Dispose(false);

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
