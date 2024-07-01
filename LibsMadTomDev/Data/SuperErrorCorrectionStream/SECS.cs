using System;
using System.IO;

namespace MadTomDev.Data
{
    public class SECS : Stream
    {
        /// <summary>
        /// 初始化超级纠错流的实例
        /// </summary>
        /// <param name="blockWidth">每数据块的宽度必须为4的倍数；高度==宽度，验证数据量为宽度*3；宽度越小容错越好，但利用率降低；</param>
        /// <param name="bandWidth">一组数据带的数据块数量，数量越多，可容纳的连续错误数据长度越长；</param>
        public SECS(Stream secStream, byte blockDataWidth = 3, int bandWidth = 512)
        {
            this.secStream = secStream;

            // basic checks
            if (!Block.CheckDataWidth(blockDataWidth, out Exception err))
                throw err;
            if (secStream == null)
                throw new NullReferenceException("The raw-stream must not be null.");
            if (bandWidth < 1)
                throw new ArgumentException("Band width must be at least 1.");

            // get basic index
            BlockDataWidth = blockDataWidth;
            BlockDataLength = blockDataWidth * blockDataWidth;
            BlockFullLength = (blockDataWidth + 3) * blockDataWidth;

            BandWidth = bandWidth;
            BandFullLength = BlockFullLength * bandWidth;
            BandDataLength = BlockDataLength * bandWidth;
            BandDataLength_noLengthData = BandDataLength - Band.lengthBytesLength;

            // load raw stream length
            loaderBand = new Band(blockDataWidth, bandWidth);
            if (CanSeek)
            {
                long totalBands_dec1 = secStream.Length / BandFullLength - 1;
                long tmp = totalBands_dec1 * BandDataLength_noLengthData;
                LoadBandFromSECS(totalBands_dec1);
                loaderBand.blocks[0].TryCorrecting(true);
                _Length = tmp + loaderBand.LoadLengthData();
            }
            else
            {
                _Length = 0;
            }
        }

        private Stream secStream;

        #region basic index
        public int BlockDataWidth { private set; get; }
        public int BlockDataLength { private set; get; }

        /// <summary>
        /// 一个数据块的完整(字节)长度，包括原数据和纠错码；
        /// </summary>
        public int BlockFullLength { private set; get; }
        /// <summary>
        /// 一个条带包含多少个数据块；
        /// </summary>
        public int BandWidth { private set; get; }

        public int BandFullLength { private set; get; }
        public int BandDataLength { private set; get; }
        public int BandDataLength_noLengthData { private set; get; }

        #endregion

        public override bool CanRead => secStream.CanRead;

        public override bool CanSeek => secStream.CanSeek;

        public override bool CanWrite => secStream.CanWrite;

        private long _Length = 0;
        public override long Length => _Length;
        public override void SetLength(long value)
        {
            _Length = value;
        }

        private long _Position = 0;
        public override long Position
        {
            get => _Position;
            set
            {
                if (_Position < 0)
                    throw new ArgumentOutOfRangeException("Position", "Position must be greator than 0.");
                if (_Position > _Length)
                    throw new ArgumentOutOfRangeException("Position", "Position must be within Length");
                _Position = value;
            }
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.End:
                    Position = _Length + offset;
                    break;
            }
            return _Position;
        }

        private long loaderBandIndex = -1;
        private Band loaderBand;
        private bool LoadBandFromSECS(long startBand, bool forceReload = false)
        {
            if (loaderBandIndex == startBand && !forceReload)
                return true;

            long iPosi = startBand * BandFullLength;
            if (iPosi >= secStream.Length)
                return false;
            secStream.Position = iPosi;
            int readLength = secStream.Read(loaderBand.bandBuffer, 0, BandFullLength);
            if (readLength != BandFullLength)
                throw new DataMisalignedException("Read data insufficient.");
            loaderBand.LoadBlocksFromBandBuffer();
            //loaderBand.TryCurrect();
            loaderBandIndex = startBand;
            return true;
        }

        /// <summary>
        /// 从纠错流中获取(纠错后的)原始数据
        /// </summary>
        /// <param name="buffer">保存读出的数据的数组</param>
        /// <param name="offset">从buffer的offset位置开始写入读出的数据</param>
        /// <param name="count">要求读出数据的长度</param>
        /// <returns>实际读出数据的长度</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            long bandIdx = _Position / BandDataLength_noLengthData;
            int bandLength = (count + BandDataLength_noLengthData - 1) / BandDataLength_noLengthData;
            int i = (int)(_Position % BandDataLength_noLengthData);
            int iv;

            int iCount = count;
            for (int bi = 0; bi < bandLength; bi++)
            {
                if (!LoadBandFromSECS(bandIdx + bi))
                    break;
                if (!loaderBand.TryCurrect())
                    throw new Exception($"Data cruption at band [{bi}].");
                loaderBand.LoadBandDataFromBlocks();
                for (iv = loaderBand.LoadLengthData(); i < iv; i++)
                {
                    buffer[offset++] = loaderBand.bandData[i];
                    iCount--;
                    if (iCount <= 0)
                        break;
                }
                i = 0;
            }
            int readCount = count - iCount;
            _Position += readCount;
            return readCount;
        }



        /// <summary>
        /// 将原始数据中写入到纠错流(纠错后的)
        /// </summary>
        /// <param name="buffer">向流中写入数据的来源</param>
        /// <param name="offset">从缓存的哪个位置开始读取要写入的数据</param>
        /// <param name="count">要写入的长度</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            long bandIdx = _Position / BandDataLength_noLengthData;
            int bandLength = (count + BandDataLength_noLengthData) / BandDataLength_noLengthData;

            int i = (int)(_Position % BandDataLength_noLengthData);
            int writeLen = Math.Min(count, BandDataLength_noLengthData - i);
            long secBandStart;
            for (int bi = 0; bi < bandLength; bi++)
            {
                secBandStart = (bandIdx + bi) * BandFullLength;
                if (secBandStart >= secStream.Length)
                {
                    // add new band
                    loaderBandIndex = bandIdx + bi;
                    loaderBand.Clear();
                }
                else
                {
                    // to exist band
                    LoadBandFromSECS(bandIdx + bi);
                    if (!loaderBand.TryCurrect())
                        throw new Exception($"Data cruption at band [{bi}].");
                }
                loaderBand.Position = i;
                loaderBand.Write(buffer, offset, writeLen);
                loaderBand.WriteLength();
                loaderBand.GenerateCCs();
                loaderBand.FlushToBandBuffer();

                secStream.Position = secBandStart;
                secStream.Write(loaderBand.bandBuffer, 0, BandFullLength);

                _Position += writeLen;
                offset += writeLen;
                if (_Length < _Position)
                    _Length = _Position;

                i = 0;
                count -= writeLen;
                if (count <= 0)
                    break;
                writeLen = Math.Min(count, BandDataLength_noLengthData);
            }
        }
        public override void Flush()
        {
            secStream.Flush();
        }

        public void CopyTo_optimized(Stream targetStream)
        {
            int bufferLen = BandDataLength_noLengthData - (int)(Position % BandDataLength_noLengthData);
            byte[] buffer = new byte[BandDataLength_noLengthData];
            int readLength;
            do
            {
                readLength = Read(buffer, 0, bufferLen);
                targetStream.Write(buffer, 0, readLength);
                bufferLen = BandDataLength_noLengthData;
            }
            while (readLength > 0);
        }
        public void CopyTo_optimized(SECS targetStream)
        {
            int sourceBufferLen = BandDataLength_noLengthData - (int)(Position % BandDataLength_noLengthData);
            byte[] buffer = new byte[this.BandDataLength_noLengthData + targetStream.BandDataLength_noLengthData];
            int writeStart = 0, offset = 0;

            int targetBufferLen = targetStream.BandDataLength_noLengthData - (int)(targetStream.Position % targetStream.BandDataLength_noLengthData);

            int readLength, i, j;
            bool isInitWrite = true, isInitRead = true;
            while (true)
            {
                readLength = Read(buffer, offset, sourceBufferLen);
                if (readLength <= 0)
                    break;

                offset += readLength;
                while ((offset - writeStart) >= targetBufferLen)
                {
                    // 当缓存中有足够写目标的数据长度后，开始向目标写入
                    // 如果前几次读取的长度不够，则不执行下面代码，继续循环读取；
                    targetStream.Write(buffer, writeStart, targetBufferLen);
                    writeStart += targetBufferLen;
                    if (isInitWrite)
                    {
                        targetBufferLen = targetStream.BandDataLength_noLengthData;
                        isInitWrite = false;
                    }
                }
                if (writeStart > 0)
                {
                    // 将缓冲中没有写入的数据，转移到最前面
                    for (i = 0, j = writeStart; j < offset; i++, j++)
                    {
                        buffer[i] = buffer[j];
                    }
                    writeStart = 0;
                    offset = i;
                }

                if (isInitRead)
                {
                    sourceBufferLen = BandDataLength_noLengthData;
                }
            }

            if (offset > 0)
            {
                targetStream.Write(buffer, 0, offset);
            }
        }
        public void CopyFrom_optimized(Stream sourceStream)
        {
            byte[] buffer = new byte[BandDataLength_noLengthData ];
            int bufferLen = BandDataLength_noLengthData - (int)(Position % BandDataLength_noLengthData);
            int readLength;
            bool isInit = true;
            while (true)
            {
                readLength = sourceStream.Read(buffer, 0, bufferLen);
                if (readLength <= 0)
                    break;
                Write(buffer, 0, readLength);
                if (isInit)
                {
                    bufferLen = BandDataLength_noLengthData;
                    isInit = false;
                }
            }
        }
        public void CopyFrom_optimized(SECS sourceStream)
        {
            sourceStream.CopyTo_optimized(this);
        }
    }

}
