using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace blqw.Unitable.Tests
{
    public class NamedUnitAddTests
    {
        [Fact]
        public void NamedUnitAdd()
        {
            var name = "blqw.zzj";
            var group = "blqw";
            var unit = new NamedUnit(name);
            var observer = new TestObserver();
            unit.Subscribe(observer);

            var u1 = new TestUnit { Name = name, GroupName = group, Priority = 1, Verion = 1 };
            var u2 = new TestUnit { Name = name, GroupName = group, Priority = 1, Verion = 2 };
            var u3 = new TestUnit { Name = name, GroupName = group, Priority = 2, Verion = 2 };
            var u4 = new TestUnit { Name = "x", GroupName = group, Priority = 1, Verion = 1 };
            var u5 = new TestUnit { Name = name, GroupName = "x", Priority = 1, Verion = 1 };
            var u6 = new TestUnit { Name = name, GroupName = group, Priority = 1, Verion = 2 };

            Assert.True(unit.Add(u1));
            Assert.Equal("OnNext", observer.LastMethod);
            Assert.Equal(UnitEventType.Add, observer.LastEventType);
            Assert.Equal(u1, observer.LastUnit);
            Assert.Equal(u1, unit.DefaultUnit);
            Assert.Equal(u1, unit[1]);
            Assert.Null(unit[2]);
            Assert.Equal(1, unit.Count);

            Assert.True(unit.Add(u2));
            Assert.Equal("OnNext", observer.LastMethod);
            Assert.Equal(UnitEventType.Add, observer.LastEventType);
            Assert.Equal(u2, observer.LastUnit);
            Assert.Equal(u2, unit.DefaultUnit);
            Assert.Equal(u1, unit[1]);
            Assert.Equal(u2, unit[2]);

            Assert.True(unit.Add(u6));
            Assert.Equal("OnNext", observer.LastMethod);
            Assert.Equal(UnitEventType.Replace, observer.LastEventType);
            Assert.Equal(u6, observer.LastUnit);
            Assert.Equal(u6, unit.DefaultUnit);
            Assert.Equal(u1, unit[1]);
            Assert.Equal(u6, unit[2]);
            Assert.True(u2.IsDisposed);
            Assert.Equal(2, unit.Count);

            Assert.True(unit.Add(u3));
            Assert.Equal("OnNext", observer.LastMethod);
            Assert.Equal(UnitEventType.Replace, observer.LastEventType);
            Assert.Equal(u3, observer.LastUnit);
            Assert.Equal(u3, unit.DefaultUnit);
            Assert.Equal(u1, unit[1]);
            Assert.Equal(u3, unit[2]);
            Assert.True(u6.IsDisposed);
            Assert.Equal(2, unit.Count);


            Assert.False(unit.Add(u4));
            Assert.False(unit.Add(u5));

            unit.Dispose();
            Assert.Equal("OnCompleted", observer.LastMethod);

            Assert.Throws<ObjectDisposedException>(() => unit.Add(u4));
            Assert.Null(unit[1]);
            Assert.Equal(0, unit.Count);
            Assert.Null(unit.DefaultUnit);
            Assert.Equal(-1, unit.Priority);
            Assert.Equal(0, unit.Verion);
            Assert.Throws<ObjectDisposedException>(() => unit.Subscribe(observer));
            unit.Dispose();
            Assert.True(u1.IsDisposed);
            Assert.True(u2.IsDisposed);
            Assert.True(u3.IsDisposed);
        }
        
        class TestUnit : IUnit
        {
            public string Name { get; set; }

            public string GroupName { get; set; }

            public decimal Verion { get; set; }

            public int Priority { get; set; }

            public (bool success, object value) TryConvert(Type type) => throw new NotImplementedException();
            public void Dispose() { IsDisposed = true; }

            public bool IsDisposed { get; set; }
        }

        class TestObserver : IObserver<(UnitEventType type, IUnit unit)>
        {
            public UnitEventType LastEventType { get; private set; }
            public IUnit LastUnit { get; private set; }
            public Exception LastError { get; private set; }

            public string LastMethod { get; private set; }

            public void OnCompleted()
            {
                LastMethod = "OnCompleted";
                LastEventType = 0;
                LastUnit = null;
                LastError = null;
            }
            public void OnError(Exception error)
            {
                LastMethod = "OnError";
                LastEventType = 0;
                LastUnit = null;
                LastError = error;
            }
            public void OnNext((UnitEventType type, IUnit unit) value)
            {
                LastMethod = "OnNext";
                LastEventType = value.type;
                LastUnit = value.unit;
                LastError = null;
            }
        }
    }
}
