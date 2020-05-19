using System;
using System.Linq;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class DataPacketTests
    {
        class TestPacket : NVorbis.DataPacket
        {
            internal byte[] _data;
            int _offset;
            System.Reflection.PropertyInfo _isShortPropInfo;

            public TestPacket(byte[] data)
                : base(data.Length)
            {
                _data = data;

                _isShortPropInfo = this.GetType().BaseType.GetProperty("IsShort", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
            }

            protected override int ReadNextByte()
            {
                if (_offset < _data.Length)
                {
                    return _data[_offset++];
                }
                return -1;
            }

            public void Reset()
            {
                base.ResetBitReader();

                _offset = 0;
            }

            public bool IsShort
            {
                get { return (bool)_isShortPropInfo.GetValue(this, null); }
            }
        }

        TestPacket _defaultTestPacket;

        [TestFixtureSetUp]
        public void Init()
        {
            _defaultTestPacket = new TestPacket(new byte[] { 0xA5, 0xB6, 0xC7, 0xD8, 0xE9, 0xFA, 0x0B, 0x1C, 0x2D, 0x3E, 0x4F, 0x50 });
        }

        [SetUp]
        public void Reset()
        {
            _defaultTestPacket.Reset();
        }

        #region PeekBits

        [Test]
        public void PeekBitsShouldErrorOnBadCount()
        {
            int temp;
            Assert.Throws<ArgumentOutOfRangeException>(() => _defaultTestPacket.TryPeekBits(66, out temp));
        }

        [Test]
        public void PeekBitsShouldErrorOnBadCount2()
        {
            int temp;
            Assert.Throws<ArgumentOutOfRangeException>(() => _defaultTestPacket.TryPeekBits(-1, out temp));
        }

        [Test]
        public void PeekBitsShouldReturnZeroOnZeroCount()
        {
            int temp;
            var output = _defaultTestPacket.TryPeekBits(0, out temp);

            Assert.AreEqual(0, temp);
            Assert.AreEqual(0, output);
        }

        [Test]
        public void PeekBitsPartialByte()
        {
            int temp;
            var output = _defaultTestPacket.TryPeekBits(3, out temp);

            Assert.AreEqual(3, temp);
            Assert.AreEqual(5, output);
        }

        [Test]
        public void PeekBitsFullByte()
        {
            int temp;
            var output = _defaultTestPacket.TryPeekBits(8, out temp);

            Assert.AreEqual(8, temp);
            Assert.AreEqual(0xA5, output);
        }

        [Test]
        public void PeekBitsMoreThanByte()
        {
            int temp;
            var output = _defaultTestPacket.TryPeekBits(11, out temp);

            Assert.AreEqual(temp, 11);
            Assert.AreEqual(output, 0x6A5);
        }

        [Test]
        public void PeekBitsPastEnd()
        {
            _defaultTestPacket.SkipBits(80);

            int temp;
            var output = _defaultTestPacket.TryPeekBits(32, out temp);

            Assert.AreEqual(16, temp);
            Assert.AreEqual(0x504F, output);
            Assert.AreEqual(80, _defaultTestPacket.BitsRead);
            Assert.AreEqual(true, _defaultTestPacket.IsShort);
        }

        [Test]
        public void PeekBitsWithOverflow()
        {
            _defaultTestPacket.SkipBits(6);

            int temp;
            var output = _defaultTestPacket.TryPeekBits(64, out temp);

            Assert.AreEqual(64, temp);
            Assert.AreEqual(0x1C0BFAE9D8C7B6A5UL >> 6 | 0x2DUL << 58, output);
        }

        [Test]
        public void PeekBitsRecoverOverflow()
        {
            _defaultTestPacket.SkipBits(6);

            int temp;
            _defaultTestPacket.TryPeekBits(64, out temp);

            _defaultTestPacket.SkipBits(8);
            var output = _defaultTestPacket.TryPeekBits(58, out temp);

            Assert.AreEqual(58, temp);
            Assert.AreEqual(14, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xB4702FEBA7631E, output);
        }

        #endregion

        #region SkipBits

        [Test]
        public void SkipBitsZero()
        {
            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(0);

            Assert.AreEqual(startBits, _defaultTestPacket.BitsRead);
        }

        [Test]
        public void SkipBitsLessThanBitCount()
        {
            int temp;
            _defaultTestPacket.TryPeekBits(6, out temp);

            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(4);

            Assert.AreEqual(startBits + 4, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xA, _defaultTestPacket.TryPeekBits(4, out temp));
        }

        [Test]
        public void SkipBitsEqualToBitCount()
        {
            int temp;
            _defaultTestPacket.TryPeekBits(6, out temp);

            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(6);

            Assert.AreEqual(startBits + 6, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xA, _defaultTestPacket.TryPeekBits(4, out temp));
        }

        [Test]
        public void SkipBitsWithOverflow1()
        {
            _defaultTestPacket.SkipBits(6);

            int temp;
            _defaultTestPacket.TryPeekBits(64, out temp);

            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(1);

            Assert.AreEqual(startBits + 1, _defaultTestPacket.BitsRead);

            var testVal = _defaultTestPacket.TryPeekBits(8, out temp);

            _defaultTestPacket.Reset();
            _defaultTestPacket.SkipBits(7);
            Assert.AreEqual(_defaultTestPacket.TryPeekBits(8, out temp), testVal);
        }

        [Test]
        public void SkipBitsWithOverflow2()
        {
            _defaultTestPacket.SkipBits(6);

            int temp;
            _defaultTestPacket.TryPeekBits(64, out temp);

            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(2);

            Assert.AreEqual(startBits + 2, _defaultTestPacket.BitsRead);

            var testVal = _defaultTestPacket.TryPeekBits(8, out temp);

            _defaultTestPacket.Reset();
            _defaultTestPacket.SkipBits(8);
            Assert.AreEqual(_defaultTestPacket.TryPeekBits(8, out temp), testVal);
        }

        [Test]
        public void SkipBitsGreaterThanBitCountNoRead()
        {
            int temp;
            _defaultTestPacket.TryPeekBits(6, out temp);

            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(8);

            Assert.AreEqual(startBits + 8, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xB6, _defaultTestPacket.TryPeekBits(8, out temp));
        }

        [Test]
        public void SkipBitsGreaterThanBitCountWithRead1()
        {
            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(16);

            int temp;
            Assert.AreEqual(startBits + 16, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xC7, _defaultTestPacket.TryPeekBits(8, out temp));
        }

        [Test]
        public void SkipBitsGreaterThanBitCountWithRead2()
        {
            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(18);

            int temp;
            Assert.AreEqual(startBits + 18, _defaultTestPacket.BitsRead);
            Assert.AreEqual((0x8C7 >> 2) & 0xFF, _defaultTestPacket.TryPeekBits(8, out temp));
        }

        [Test]
        public void SkipBitsGreaterThanBitCountWithOverflow()
        {
            _defaultTestPacket.SkipBits(6);

            int temp;
            _defaultTestPacket.TryPeekBits(64, out temp);

            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(64);

            Assert.AreEqual(startBits + 64, _defaultTestPacket.BitsRead);

            var testVal = _defaultTestPacket.TryPeekBits(8, out temp);

            _defaultTestPacket.Reset();
            _defaultTestPacket.SkipBits(70);
            Assert.AreEqual(_defaultTestPacket.TryPeekBits(8, out temp), testVal);
        }

        [Test]
        public void SkipBitsGreaterThanBitCountWithOverflowRead()
        {
            _defaultTestPacket.SkipBits(6);

            int temp;
            _defaultTestPacket.TryPeekBits(64, out temp);

            _defaultTestPacket.SkipBits(_defaultTestPacket.Length * 8);

            Assert.AreEqual(_defaultTestPacket.Length * 8, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0, _defaultTestPacket.TryPeekBits(8, out temp));
        }

        [Test]
        public void SkipBitsGreaterThanLength1()
        {
            var startBits = _defaultTestPacket.BitsRead;
            _defaultTestPacket.SkipBits(_defaultTestPacket.Length * 8 + 16);

            int temp;
            Assert.AreEqual(startBits + _defaultTestPacket.Length * 8, _defaultTestPacket.BitsRead);
            Assert.AreEqual(true, _defaultTestPacket.IsShort);
            Assert.AreEqual(0, _defaultTestPacket.TryPeekBits(8, out temp));
        }
        
        #endregion

        #region ResetBitReader

        [Test]
        public void ResetBitReader()
        {
            _defaultTestPacket.SkipBits(17);
            int temp;
            _defaultTestPacket.TryPeekBits(7, out temp);

            _defaultTestPacket.Reset();

            Assert.AreEqual(0, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xA5, _defaultTestPacket.TryPeekBits(8, out temp));
        }

        #endregion

        #region ReadBits

        [Test]
        public void ReadBitsZero()
        {
            var val = _defaultTestPacket.ReadBits(0);

            Assert.AreEqual(0, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0, val);
        }

        [Test]
        public void ReadBitsPartialByte()
        {
            var val = _defaultTestPacket.ReadBits(7);

            Assert.AreEqual(7, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xA5 & 0x7F, val);
        }

        // there's really no need to test this extensively since PeekBits & SkipBits do all the real work

        #endregion

        #region PeekByte

        [Test]
        public void PeekByte()
        {
            var val = _defaultTestPacket.PeekByte();

            Assert.AreEqual(0, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xA5, val);
        }

        #endregion

        #region ReadByte

        [Test]
        public void ReadByte()
        {
            var val = _defaultTestPacket.ReadByte();

            Assert.AreEqual(8, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xA5, val);
        }

        #endregion

        #region ReadBytes
        
        [Test]
        public void ReadBytesZero()
        {
            var val = _defaultTestPacket.ReadBytes(0);

            Assert.AreEqual(0, _defaultTestPacket.BitsRead);
            Assert.IsNotNull(val);
            Assert.AreEqual(0, val.Length);
        }

        [Test]
        public void ReadBytesMultiple()
        {
            var val = _defaultTestPacket.ReadBytes(4);

            Assert.AreEqual(32, _defaultTestPacket.BitsRead);
            Assert.IsNotNull(val);
            Assert.AreEqual(4, val.Length);
            Assert.That(val.SequenceEqual(_defaultTestPacket._data.Take(4)));
        }
        
        #endregion

        #region Read

        [Test]
        public void ReadShouldErrorOnBadIndex()
        {
            var buf = new byte[7];
            Assert.Throws<ArgumentOutOfRangeException>(() => _defaultTestPacket.Read(buf, -1, 5));
        }

        [Test]
        public void ReadShouldErrorOnShortBuffer()
        {
            var buf = new byte[7];
            Assert.Throws<ArgumentOutOfRangeException>(() => _defaultTestPacket.Read(buf, 0, 8));
        }

        [Test]
        public void ReadZero()
        {
            var buf = new byte[8];
            var cnt = _defaultTestPacket.Read(buf, 0, 0);

            Assert.AreEqual(0, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0, cnt);
        }

        [Test]
        public void ReadMultiple()
        {
            var buf = new byte[4];
            var cnt = _defaultTestPacket.Read(buf, 0, 4);

            Assert.AreEqual(32, _defaultTestPacket.BitsRead);
            Assert.AreEqual(4, cnt);
            Assert.That(buf.SequenceEqual(_defaultTestPacket._data.Take(4)));
        }

        [Test]
        public void ReadMultipleWithOffset()
        {
            var buf = new byte[8];
            var cnt = _defaultTestPacket.Read(buf, 4, 4);

            Assert.AreEqual(32, _defaultTestPacket.BitsRead);
            Assert.AreEqual(4, cnt);
            Assert.That(buf.Skip(4).SequenceEqual(_defaultTestPacket._data.Take(4)));
        }

        [Test]
        public void ReadPastEnd()
        {
            var buf = new byte[_defaultTestPacket.Length + 1];
            var cnt = _defaultTestPacket.Read(buf, 0, buf.Length);

            Assert.AreEqual(_defaultTestPacket.Length * 8, _defaultTestPacket.BitsRead);
            Assert.AreEqual(_defaultTestPacket.Length, cnt);
            Assert.That(buf.Take(_defaultTestPacket.Length).SequenceEqual(_defaultTestPacket._data));
        }

        #endregion

        #region ReadBit

        [Test]
        public void ReadBit1()
        {
            var val = _defaultTestPacket.ReadBit();

            Assert.AreEqual(1, _defaultTestPacket.BitsRead);
            Assert.AreEqual(true, val);
        }

        [Test]
        public void ReadBit2()
        {
            _defaultTestPacket.SkipBits(1);
            var val = _defaultTestPacket.ReadBit();

            Assert.AreEqual(2, _defaultTestPacket.BitsRead);
            Assert.AreEqual(false, val);
        }

        #endregion

        #region ReadInt16

        [Test]
        public void ReadInt16()
        {
            var val = _defaultTestPacket.ReadInt16();

            Assert.AreEqual(16, _defaultTestPacket.BitsRead);
            Assert.AreEqual(-18779, val);
        }

        #endregion

        #region ReadInt32

        [Test]
        public void ReadInt32()
        {
            var val = _defaultTestPacket.ReadInt32();

            Assert.AreEqual(32, _defaultTestPacket.BitsRead);
            Assert.AreEqual(-658000219, val);
        }

        #endregion

        #region ReadInt64

        [Test]
        public void ReadInt64()
        {
            var val = _defaultTestPacket.ReadInt64();

            Assert.AreEqual(64, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0x1C0BFAE9D8C7B6A5, val);
        }

        #endregion

        #region ReadUInt16

        [Test]
        public void ReadUInt16()
        {
            var val = _defaultTestPacket.ReadUInt16();

            Assert.AreEqual(16, _defaultTestPacket.BitsRead);
            Assert.AreEqual((ushort)0xB6A5, val);
        }

        #endregion

        #region ReadUInt32

        [Test]
        public void ReadUInt32()
        {
            var val = _defaultTestPacket.ReadUInt32();

            Assert.AreEqual(32, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0xD8C7B6A5U, val);
        }

        #endregion

        #region ReadUInt64

        [Test]
        public void ReadUInt64()
        {
            var val = _defaultTestPacket.ReadUInt64();

            Assert.AreEqual(64, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0x1C0BFAE9D8C7B6A5UL, val);
        }

        #endregion

        #region SkipBytes

        [Test]
        public void SkipBytesZero()
        {
            _defaultTestPacket.SkipBytes(0);

            Assert.AreEqual(0, _defaultTestPacket.BitsRead);
        }

        [Test]
        public void SkipBytesMultiple()
        {
            _defaultTestPacket.SkipBytes(6);

            Assert.AreEqual(48, _defaultTestPacket.BitsRead);
            Assert.AreEqual(0x0B, _defaultTestPacket.ReadByte());
        }

        #endregion
    }
}
