using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using TestHarness;

namespace UnitTests
{
    class StreamReadBufferTests
    {
        //Stream GetStandardStream()
        //{
        //    var buf = new byte[4096];
        //    for (int i = 0; i < buf.Length; i++)
        //    {
        //        buf[i] = (byte)((i / 2 >> ((i % 2) * 8)) & 0xFF);
        //    }
        //    return new MemoryStream(buf);
        //}

        //StreamReadBuffer GetStandardWrapper(Stream testStream)
        //{
        //    return new StreamReadBuffer(testStream, 512, 2048, false);
        //}

        //#region .ctor

        //// technically, we should check to make sure the stream wrapper logic works...
        //// but we won't here

        //// base constructor
        //[Test]
        //public void Constructor0()
        //{
        //    using (var testingStream = GetStandardStream())
        //    {
        //        var wrapper = new StreamReadBuffer(testingStream, 47, 84, false);

        //        Assert.AreEqual(64, wrapper.TestingInitialSize);
        //        Assert.AreEqual(64, wrapper.MaxSize);
        //        Assert.IsTrue(wrapper.TestingBuffer.Length == 64);
        //    }
        //}

        //#endregion

        //#region ReadStream

        ////   readCount == 0
        //[Test]
        //public void ReadStream0()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        wrapper.TestingReadStream(0, 0, 0L);

        //        Assert.AreEqual(0, testStream.Position);
        //        Assert.AreEqual(0, wrapper.TestingBufferEndIndex);
        //    }
        //}
        ////   readCount > 0 && readOffset + readCount < _wrapper.EofOffset
        //[Test]
        //public void ReadStream1()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = testStream.Length - 11;
        //        wrapper.TestingReadStream(0, 10, testStream.Position);

        //        Assert.AreEqual(testStream.Length - 1, testStream.Position);
        //        Assert.AreEqual(10, wrapper.TestingBufferEndIndex);
        //        Assert.That(wrapper.TestingBuffer.Take(10).SequenceEqual(new byte[] { 7, 251, 7, 252, 7, 253, 7, 254, 7, 255 }));
        //    }
        //}
        ////   readCount > 0 && readOffset + readCount == _wrapper.EofOffset
        //[Test]
        //public void ReadStream2()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = testStream.Length - 10;
        //        wrapper.TestingReadStream(0, 10, testStream.Position);

        //        Assert.AreEqual(testStream.Length, testStream.Position);
        //        Assert.AreEqual(10, wrapper.TestingBufferEndIndex);
        //        Assert.That(wrapper.TestingBuffer.Take(10).SequenceEqual(new byte[] { 251, 7, 252, 7, 253, 7, 254, 7, 255, 7 }));
        //    }
        //}
        ////   readCount > 0 && readOffset + readCount > _wrapper.EofOffset
        //[Test]
        //public void ReadStream3()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = testStream.Length - 9;
        //        wrapper.TestingReadStream(0, 10, testStream.Position);

        //        Assert.AreEqual(testStream.Length, testStream.Position);
        //        Assert.AreEqual(9, wrapper.TestingBufferEndIndex);
        //        // check that the read data matches our expectation
        //        Assert.That(wrapper.TestingBuffer.Take(9).SequenceEqual(new byte[] { 7, 252, 7, 253, 7, 254, 7, 255, 7 }));
        //    }
        //}
        ////   readCount > 0 && readOffset + readCount > _wrapper.EofOffset (unknown value)
        //[Test]
        //public void ReadStream3a()
        //{
        //    using (var testStream = GetStandardStream())
        //    using (var foStream = new ForwardOnlyStream(testStream))
        //    {
        //        var wrapper = GetStandardWrapper(foStream);
        //        testStream.Position = testStream.Length - 9;
        //        wrapper.TestingReadStream(0, 10, testStream.Position);

        //        Assert.AreEqual(testStream.Length, testStream.Position);
        //        Assert.AreEqual(9, wrapper.TestingBufferEndIndex);
        //        Assert.That(wrapper.TestingBuffer.Take(9).SequenceEqual(new byte[] { 7, 252, 7, 253, 7, 254, 7, 255, 7 }));
        //    }
        //}
        ////   readOffset == _wrapper.EofOffset
        //[Test]
        //public void ReadStream4()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = testStream.Length;
        //        wrapper.TestingReadStream(0, 10, testStream.Position);

        //        Assert.AreEqual(testStream.Length, testStream.Position);
        //        Assert.AreEqual(0, wrapper.TestingBufferEndIndex);
        //    }
        //}
        ////   readOffset > _wrapper.EofOffset
        //[Test]
        //public void ReadStream5()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = testStream.Length;
        //        wrapper.TestingReadStream(0, 10, testStream.Position + 1);

        //        Assert.AreEqual(testStream.Length, testStream.Position);
        //        Assert.AreEqual(0, wrapper.TestingBufferEndIndex);
        //    }
        //}

        //#endregion

        //#region PrepareStreamForRead

        ////   readCount == 0
        //[Test]
        //public void PrepareStreamForRead0()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = 50;

        //        int readCount = wrapper.TestingPrepareStreamForRead(0, 55);

        //        Assert.AreEqual(0, readCount);
        //        Assert.AreEqual(50, testStream.Position);
        //    }
        //}
        ////   _wrapper.Source.Position == readOffset
        //[Test]
        //public void PrepareStreamForRead1()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = 50;

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 0);

        //        Assert.AreEqual(1, readCount);
        //        Assert.AreEqual(0, testStream.Position);
        //    }
        //}
        ////   _wrapper.Source.Position != readOffset && readoffset < _wrapper.EofOffset
        ////     _wrapper.Source.CanSeek == false
        ////       past end of stream
        //[Test]
        //public void PrepareStreamForRead2a()
        //{
        //    using (var testStream = GetStandardStream())
        //    using (var foStream = new ForwardOnlyStream(testStream))
        //    {
        //        var wrapper = GetStandardWrapper(foStream);

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 5000);

        //        Assert.AreEqual(0, readCount);
        //        Assert.AreEqual(4096, testStream.Position);
        //    }
        //}
        ////       to place in stream
        //[Test]
        //public void PrepareStreamForRead2b()
        //{
        //    using (var testStream = GetStandardStream())
        //    using (var foStream = new ForwardOnlyStream(testStream))
        //    {
        //        var wrapper = GetStandardWrapper(foStream);

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 55);

        //        Assert.AreEqual(1, readCount);
        //        Assert.AreEqual(55, testStream.Position);
        //    }
        //}
        ////       before current position
        //[Test]
        //public void PrepareStreamForRead2c()
        //{
        //    using (var testStream = GetStandardStream())
        //    using (var foStream = new ForwardOnlyStream(testStream))
        //    {
        //        var wrapper = GetStandardWrapper(foStream);
        //        testStream.Position = 50;

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 0);

        //        Assert.AreEqual(0, readCount);
        //        Assert.AreEqual(50, testStream.Position);
        //    }
        //}
        ////     _wrapper.Source.CanSeek == true
        ////       readOffset > _wrapper.Source.Position
        //[Test]
        //public void PrepareStreamForRead3a()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 55);

        //        Assert.AreEqual(1, readCount);
        //        Assert.AreEqual(55, testStream.Position);
        //    }
        //}
        ////       readOffset < _wrapper.Source.Position
        //[Test]
        //public void PrepareStreamForRead3b()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);
        //        testStream.Position = 50;

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 0);

        //        Assert.AreEqual(1, readCount);
        //        Assert.AreEqual(0, testStream.Position);
        //    }
        //}
        ////   _wrapper.Source.Position != readOffset && readoffset == _wrapper.EofOffset
        //[Test]
        //public void PrepareStreamForRead4a()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 4096);

        //        Assert.AreEqual(0, readCount);
        //        Assert.AreEqual(0, testStream.Position);
        //    }
        //}
        ////   _wrapper.Source.Position != readOffset && readoffset > _wrapper.EofOffset
        //[Test]
        //public void PrepareStreamForRead4b()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);

        //        int readCount = wrapper.TestingPrepareStreamForRead(1, 4097);

        //        Assert.AreEqual(0, readCount);
        //        Assert.AreEqual(0, testStream.Position);
        //    }
        //}

        //#endregion

        //#region FillBuffer

        //// read next bytes into next section of buffer (beginning)
        //// read next bytes into next section of buffer (middle)
        //// read next bytes into section of buffer (beginning)
        //// read previous bytes into section of buffer (beginning)

        //#endregion

        //#region EnsureAvailable

        //// startIdx >= 0 && endIdx <= _end
        //[Test]
        //public void EnsureAvailable0()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);

        //        wrapper.TestingBufferEndIndex = 10;

        //        int count = 2;

        //        var index = wrapper.TestingEnsureAvailable(5L, ref count);

        //        Assert.AreEqual(5, index);
        //        Assert.AreEqual(2, count);
        //    }
        //}
        //// !(startIdx >= 0 && endIdx <= _end)
        //[Test]
        //public void EnsureAvailable1()
        //{
        //    using (var testStream = GetStandardStream())
        //    {
        //        var wrapper = GetStandardWrapper(testStream);

        //        int count = 2;

        //        var index = wrapper.TestingEnsureAvailable(5L, ref count);

        //        Assert.AreEqual(5, index);
        //        Assert.AreEqual(2, count);
        //        Assert.IsTrue(wrapper.TestingBufferEndIndex + wrapper.TestingBaseOffset >= 7);
        //        Assert.AreEqual(0, wrapper.TestingBuffer[index]);
        //        Assert.AreEqual(3, wrapper.TestingBuffer[index + 1]);
        //    }
        //}

        //#endregion
    }
}
