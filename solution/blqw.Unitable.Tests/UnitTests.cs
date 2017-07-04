using System;
using Xunit;

namespace blqw.Unitable.Tests
{
    public class UnitTests
    {
        [Fact]
        public void ParsedName()
        {
            var name = "zzj";
            var group = "blqw";
            var unit = new NamedUnit($"{group}.{name}");

            Assert.Equal(group, unit.GroupName);
            Assert.Equal($"{group}.{name}", unit.Name);


            var unit2 = new NamedUnit(name);
            Assert.Equal(null, unit2.GroupName);
            Assert.Equal(name, unit2.Name);
        }


        

    }
}
