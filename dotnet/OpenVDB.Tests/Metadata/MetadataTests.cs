// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using System;
using Xunit;
using OpenVDB.Metadata;

namespace OpenVDB.Tests.Metadata
{
    public class MetadataTests
    {
        private class TestMetadata : global::OpenVDB.Metadata.Metadata
        {
            private string _value = "";

            public override string TypeName => "test";
            public override uint Size => (uint)(_value.Length + 4);
            public override global::OpenVDB.Metadata.Metadata Copy() => new TestMetadata { _value = _value };
            
            public string Value 
            { 
                get => _value;
                set => _value = value;
            }

            public override void CopyFrom(global::OpenVDB.Metadata.Metadata other)
            {
                if (other is TestMetadata tm)
                {
                    _value = tm._value;
                }
            }

            public override string AsString() => _value;
            public override bool AsBool() => !string.IsNullOrEmpty(_value);

            public override bool Equals(global::OpenVDB.Metadata.Metadata? other)
            {
                return other is TestMetadata tm && tm._value == _value;
            }

            protected override void ReadValue(System.IO.BinaryReader reader, uint numBytes)
            {
                var bytes = reader.ReadBytes((int)numBytes);
                _value = System.Text.Encoding.UTF8.GetString(bytes);
            }

            protected override void WriteValue(System.IO.BinaryWriter writer)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(_value);
                writer.Write(bytes);
            }
        }

        [Fact]
        public void TypeName_ShouldReturnCorrectType()
        {
            var meta = new TestMetadata();
            Assert.Equal("test", meta.TypeName);
        }

        [Fact]
        public void Copy_ShouldCreateIndependentCopy()
        {
            var meta1 = new TestMetadata { Value = "test" };
            var meta2 = (TestMetadata)meta1.Copy();
            
            Assert.Equal(meta1.Value, meta2.Value);
            
            meta2.Value = "changed";
            Assert.NotEqual(meta1.Value, meta2.Value);
        }

        [Fact]
        public void Equals_WithSameValue_ShouldReturnTrue()
        {
            var meta1 = new TestMetadata { Value = "test" };
            var meta2 = new TestMetadata { Value = "test" };
            
            Assert.True(meta1.Equals(meta2));
        }

        [Fact]
        public void Equals_WithDifferentValue_ShouldReturnFalse()
        {
            var meta1 = new TestMetadata { Value = "test1" };
            var meta2 = new TestMetadata { Value = "test2" };
            
            Assert.False(meta1.Equals(meta2));
        }
    }

    public class MetaMapTests
    {
        [Fact]
        public void Constructor_Default_ShouldBeEmpty()
        {
            var map = new MetaMap();
            Assert.Equal(0, map.MetaCount);
        }

        [Fact]
        public void InsertMeta_ShouldAddMetadata()
        {
            var map = new MetaMap();
            var meta = new TestMetadata { Value = "test" };
            
            map.InsertMeta("key", meta);
            
            Assert.Equal(1, map.MetaCount);
            Assert.NotNull(map["key"]);
        }

        [Fact]
        public void this_WithExistingKey_ShouldReturnMetadata()
        {
            var map = new MetaMap();
            var meta = new TestMetadata { Value = "test" };
            map.InsertMeta("key", meta);
            
            var retrieved = map["key"];
            Assert.NotNull(retrieved);
            Assert.IsType<TestMetadata>(retrieved);
        }

        [Fact]
        public void this_WithNonExistingKey_ShouldReturnNull()
        {
            var map = new MetaMap();
            var retrieved = map["nonexistent"];
            Assert.Null(retrieved);
        }

        [Fact]
        public void RemoveMeta_ShouldRemoveMetadata()
        {
            var map = new MetaMap();
            var meta = new TestMetadata { Value = "test" };
            map.InsertMeta("key", meta);
            
            Assert.NotNull(map["key"]);
            map.RemoveMeta("key");
            Assert.Null(map["key"]);
        }

        [Fact]
        public void Clear_ShouldRemoveAllMetadata()
        {
            var map = new MetaMap();
            map.InsertMeta("key1", new TestMetadata());
            map.InsertMeta("key2", new TestMetadata());
            
            Assert.Equal(2, map.MetaCount);
            map.ClearMetadata();
            Assert.Equal(0, map.MetaCount);
        }

        private class TestMetadata : global::OpenVDB.Metadata.Metadata
        {
            public string Value { get; set; } = "";
            public override string TypeName => "test";
            public override uint Size => (uint)(Value.Length + 4);
            public override global::OpenVDB.Metadata.Metadata Copy() => new TestMetadata { Value = Value };
            public override void CopyFrom(global::OpenVDB.Metadata.Metadata other) 
            {
                if (other is TestMetadata tm) Value = tm.Value;
            }
            public override string AsString() => Value;
            public override bool AsBool() => !string.IsNullOrEmpty(Value);
            public override bool Equals(global::OpenVDB.Metadata.Metadata? other) => true;
            
            protected override void ReadValue(System.IO.BinaryReader reader, uint numBytes)
            {
                var bytes = reader.ReadBytes((int)numBytes);
                Value = System.Text.Encoding.UTF8.GetString(bytes);
            }

            protected override void WriteValue(System.IO.BinaryWriter writer)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(Value);
                writer.Write(bytes);
            }
        }
    }
}
