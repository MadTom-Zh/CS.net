using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Data
{
    public class Band
    {
        public byte DataHeight { private set; get; }
        public int BandLength { private set; get; }
        public const int lengthBytesLength = 4;

        public Block[] blocks;
        private Band() { }
        public Band(byte dataHeight, int bandLength)
        {
            DataHeight = dataHeight;
            BandLength = bandLength;
            blocks = new Block[bandLength];
            Block firstBlock = new Block(dataHeight);
            blocks[0] = firstBlock;
            for (int i = 1; i < bandLength; i++)
            {
                blocks[i] = firstBlock.Clone();
            }
            MaxBandDataLength = dataHeight * dataHeight * bandLength - lengthBytesLength;
            MaxBandFullLength = (dataHeight * dataHeight + 3 * dataHeight) * bandLength;

            _DataLength = 0;
            bandBuffer = new byte[MaxBandFullLength];
            bandData = new byte[MaxBandDataLength];
        }
        public Band Clone()
        {
            Band clone = new Band()
            {
                DataHeight = DataHeight,
                BandLength = BandLength,
                MaxBandDataLength = MaxBandDataLength,
                MaxBandFullLength = MaxBandFullLength,
                _DataLength = _DataLength,
                _Position = _Position,
                HaveCCError = HaveCCError,
                bIdxer = bIdxer,
                dataIdxer = dataIdxer,
            };
            clone.blocks = new Block[BandLength];
            for (int i = 0, iv = BandLength; i < iv; i++)
                clone.blocks[i] = blocks[i].Clone();

            return clone;
        }

        public long MaxBandDataLength { private set; get; }
        public long MaxBandFullLength { private set; get; }
        private long _DataLength;
        /// <summary>
        /// the length of raw data, will auto increase when Write
        /// </summary>
        public long DataLength
        {
            get => _DataLength;
            set
            {
                if (value < 0 || value > MaxBandDataLength)
                    throw new AccessViolationException($"Length should between(equal) 0 and {MaxBandDataLength} Band(dh {DataHeight}, bl {BandLength}).");

                _DataLength = value;
                if (_Position > _DataLength)
                    Position = _DataLength;
            }
        }

        private long _Position = 0;
        public long Position
        {
            get => _Position;
            set
            {
                if (_Position == value)
                    return;
                if (_Position < 0 || _Position > MaxBandDataLength)
                    throw new IndexOutOfRangeException($"Position should between(equal) 0 and {MaxBandDataLength}");

                //ccStream.Position = 
                _Position = value;
            }
        }


        /// <summary>
        /// 将数据写入raw流；
        /// </summary>
        /// <param name="buffer">要写入的数据的缓冲</param>
        /// <param name="offset">缓冲的偏移，如offset=1时，从buffer[1]开始读取</param>
        /// <param name="count">从缓冲读取的数据量</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            if (_Position + count > MaxBandDataLength)
                throw new ArgumentOutOfRangeException(nameof(count), $"Max length {MaxBandDataLength}, current position {_Position}, write {count} bytes will cause overflow.");
            if (offset + count > buffer.Length)
                throw new IndexOutOfRangeException($"Can't get data at {offset + count}, buffer length is {buffer.Length}");

            // 定位 数据块， 定位写入位置
            int dataPosi = (int)_Position + lengthBytesLength;
            int blockDataLength = blocks[0].dataLength;
            int bIdx = (int)(dataPosi / blockDataLength);
            int bIV = BandLength;
            Block curBlock = blocks[bIdx];
            int remain = (int)(dataPosi % blockDataLength);
            int i, iv;
            int x = remain % DataHeight;
            int y = remain / DataHeight;
            for (i = offset, iv = offset + count; i < iv; i++)
            {
                curBlock.data[y][x] = buffer[i];
                dataPosi++;

                if (++x >= DataHeight)
                {
                    x = 0;
                    if (++y >= DataHeight)
                    {
                        y = 0;
                        if (++bIdx < bIV)
                            curBlock = blocks[bIdx];
                    }
                }
            }

            _Position = dataPosi - lengthBytesLength;
            if (DataLength < _Position)
                DataLength = _Position;
            // write length
            //WriteLength();
            HaveCCError = null;
        }

        internal void Clear()
        {
            Parallel.ForEach(blocks, b => b.Clear());
            DataLength = 0;
        }

        public void GenerateCCs()
        {
            Parallel.ForEach(blocks, b => b.GenerateCorrectingCode());
        }
        public void WriteLength()
        {
            byte[] lenData = BitConverter.GetBytes(_Position);

            Block curBlock = blocks[0];
            int x = 0, y = 0, i, bIdx = 0;
            for (i = 0; i < lengthBytesLength; i++)
            {
                curBlock.data[y][x] = lenData[i];
                if (++x >= DataHeight)
                {
                    x = 0;
                    if (++y >= DataHeight)
                    {
                        y = 0;
                        if (++bIdx < BandLength)
                            curBlock = blocks[bIdx];
                    }
                }
            }
        }


        private BlockIndexer bIdxer = null;
        public byte[] bandBuffer;
        public void FlushToBandBuffer()
        {
            int bufferPosi = 0;
            if (bIdxer == null)
                bIdxer = IndexerSet.GetInstance().GetBlockIndexer(DataHeight);
            int i, iv, bi, biv = blocks.Length, ccIdx, c, r;
            byte[] buffer = new byte[biv];
            for (i = 0, iv = blocks[0].fullLength; i < iv; i++)
            {
                if (bIdxer.GetPosition(i, out ccIdx, out c, out r))
                {
                    switch (ccIdx)
                    {
                        case 0:
                            for (bi = 0; bi < biv; bi++)
                            {
                                buffer[bi] = blocks[bi].ccR[r];
                            }
                            break;
                        case 1:
                            for (bi = 0; bi < biv; bi++)
                            {
                                buffer[bi] = blocks[bi].ccC[c];
                            }
                            break;
                        case 2:
                            for (bi = 0; bi < biv; bi++)
                            {
                                buffer[bi] = blocks[bi].ccA[c];
                            }
                            break;
                        default:
                            for (bi = 0; bi < biv; bi++)
                            {
                                buffer[bi] = blocks[bi].data[r][c];
                            }
                            break;
                    }
                    buffer.CopyTo(bandBuffer, bufferPosi);
                    bufferPosi += biv;
                }
                else
                    throw new IndexOutOfRangeException($"Can't get position withing Block[dh {DataHeight}] at step {i}.");
            }
        }


        public void LoadBlocksFromBandBuffer()
        {
            HaveCCError = null;
            int bandLength = BandLength;
            int bi, biv = bandLength, i, iv, ccIdx, r, c;
            int bp = 0;
            if (bIdxer == null)
                bIdxer = IndexerSet.GetInstance().GetBlockIndexer(DataHeight);
            for (i = 0, iv = blocks[0].fullLength; i < iv; i++)
            {
                bIdxer.GetPosition(i, out ccIdx, out c, out r);
                switch (ccIdx)
                {
                    case 0:
                        for (bi = 0; bi < biv; bi++, bp++)
                        {
                            blocks[bi].ccR[r] = bandBuffer[bp];
                        }
                        break;
                    case 1:
                        for (bi = 0; bi < biv; bi++, bp++)
                        {
                            blocks[bi].ccC[c] = bandBuffer[bp];
                        }
                        break;
                    case 2:
                        for (bi = 0; bi < biv; bi++, bp++)
                        {
                            blocks[bi].ccA[c] = bandBuffer[bp];
                        }
                        break;
                    default:
                        for (bi = 0; bi < biv; bi++, bp++)
                        {
                            blocks[bi].data[r][c] = bandBuffer[bp];
                        }
                        break;
                }
            }
        }

        public bool? HaveCCError { private set; get; } = null;
        public bool TryCurrect()
        {
            HaveCCError = false;
            foreach (Block b in blocks)
            {
                if (!b.TryCorrecting(true))
                {
                    HaveCCError = true;
                }
            }
            return HaveCCError == false;
        }

        private DataIndexer dataIdxer = null;
        public byte[] bandData;

        public void LoadBandDataFromBlocks()
        {
            int i, bi = 0, biv = BandLength, r = 0, c = 0, dataHeight = DataHeight;
            long iv;
            if (dataIdxer == null)
                dataIdxer = IndexerSet.GetInstance().GetDataIndexer(DataHeight);
            Block curBlock = blocks[bi];

            // skip lenth bytes
            for (i = 0; i < lengthBytesLength; i++)
            {
                if (++c >= dataHeight)
                {
                    c = 0;
                    if (++r >= dataHeight)
                    {
                        r = 0;
                        bi++;
                        curBlock = blocks[bi];
                    }
                }
            }

            // read data
            int bandLength = BandLength;
            for (i = 0, iv = MaxBandDataLength; i < iv; i++)
            {
                bandData[i] = curBlock.data[r][c];
                if (++c >= dataHeight)
                {
                    c = 0;
                    if (++r >= dataHeight)
                    {
                        r = 0;
                        if (++bi >= bandLength)
                            break;
                        curBlock = blocks[bi];
                    }
                }
            }
        }

        public int LoadLengthData()
        {
            //blocks[0].TryCorrecting(true);
            byte[] lenData = blocks[0].GetDataArray(0, lengthBytesLength);
            return BitConverter.ToInt32(lenData, 0);
        }

        public bool BlocksEqual(Block[] blocks)
        {
            if (blocks == null)
                return this.blocks == null;
            if (this.blocks == null)
                return blocks == null;
            if (this.blocks.Length != blocks.Length)
                return false;
            for (int i = 0, iv = blocks.Length; i < iv; i++)
            {
                if (blocks[i] != this.blocks[i])
                    return false;
            }
            return true;
        }
    }
}
