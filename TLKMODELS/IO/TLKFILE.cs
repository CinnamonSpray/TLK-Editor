using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TLK.IO
{
    public abstract class TLKFILE : IDisposable
    {
        #region Fields
        private bool _PreLoad = false;

        private const string BG_ChkStr = "TLK V1  ";
        private const string NW_ChkStr = "TLK V3.0";

        private string filepath { get; set; }
        private FileStream fs { get; set; } = null;
        private BinaryReader br { get; set; } = null;
        private BinaryWriter bw { get; set; } = null;

        private (int, int)[] StrPosTable { get; set; }

        public Encoding Encoder { get; set; } = Encoding.UTF8; // Encoding.GetEncoding("euc-kr");
        public uint fileDataCnt { get; private set; }
        public uint fileDataOff { get; private set; }
        public long fileLength { get; private set; }

        public string ErrorMsg { get; private set; }
        public string Version { get; private set; }
        #endregion

        public abstract int H_Size { get; } //HeaderSize
        public abstract int H_OffToStrCnt { get; } //HeaderOffsetTosStringCount
        public abstract int E_Size { get; } //EntriseSize
        public abstract int E_OffToStr { get; } //EntriseOffsetToString
        public abstract int E_OffTosStrLen { get; } //EntriseOffsetToStrintgLength
        public abstract int E_RsrcName { get; }
        public abstract int E_RsrcNameLen { get; }

        public abstract byte[] GetEntryNoText(int index);
        public abstract string EntriseToString(int index);
        public abstract bool[] CompareEntrise(byte[] left, byte[] right);

        #region Constructors
        public TLKFILE(string path)
        {
            Init(path);
        }

        public TLKFILE(string path, bool preload)
        {
            if (Init(path))
            {
                _PreLoad = preload;

                if (_PreLoad) PreLoadedStringTable();
            }
        }

        private bool Init(string path)
        {
            filepath = path;
            fileDataCnt = 0;
            fileDataOff = 0;
            fileLength = 0;

            ErrorMsg = TLKFILE_ERR.NOTHING_ERR;

            if (!IsChecked()) return false;

            fs = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
            br = new BinaryReader(fs);
            bw = new BinaryWriter(fs);

            fs.Seek(H_OffToStrCnt, SeekOrigin.Begin);
            fileDataCnt = br.ReadUInt32();
            fileDataOff = br.ReadUInt32();
            fileLength = fs.Length;

            return true;
        }
        #endregion

        public string GetText(int Index)
        {
            if (Index < 0 && Index > fileDataCnt)
                return string.Empty;

            if (_PreLoad)
            {
                fs.Seek(fileDataOff + StrPosTable[Index].Item1, SeekOrigin.Begin);

                return Encoder.GetString(br.ReadBytes(StrPosTable[Index].Item2));
            }
            else
            {
                int strIndex, strLength = 0;

                GetStrPosition(Index, out strIndex, out strLength);

                fs.Seek(fileDataOff + strIndex, SeekOrigin.Begin);

                return Encoder.GetString(br.ReadBytes(strLength));
            }
        }

        public void SetText(int Index, string text)
        {
            if (Index < 0 && Index > fileDataCnt) return;

            int oldIndex, oldLength;
            int textlength = Encoder.GetByteCount(text);

            GetStrPosition(Index, out oldIndex, out oldLength);

            // Entry 정보만 있는 경우 자를 텍스트 위치를 산출 및 Entry 정보 갱신
            if (oldIndex == 0 && oldLength == 0)
            {
                fs.Seek(EntriseInStringInfo(this, Index + 1), SeekOrigin.Begin);

                for(int i = 0; i < fileDataCnt; i++)
                {
                    oldIndex = br.ReadInt32();
                    oldLength = br.ReadInt32();

                    if (oldIndex == 0 && oldLength == 0)
                        br.ReadBytes(E_OffToStr);

                    else break;
                }

                fs.Seek(EntriseInStringInfo(this, Index), SeekOrigin.Begin);
                bw.Write(BitConverter.GetBytes(oldIndex));

                oldLength = 0;
            }

            ReplaceBytes((int)(fileDataOff + oldIndex), oldLength, Encoder.GetBytes(text), textlength);

            ModifyEntries(Index, textlength);
        }

        public void SetTextBlock(string[] mNewTexts)
        {
            int PrevTextIndex = 0;
            int CurrTextLength = 0;

            int cnt = mNewTexts.Length;
            var temp = new (int Index, int Length)[cnt];
            for (int i = 0; i < cnt; i++)
            {
                CurrTextLength = Encoder.GetByteCount(mNewTexts[i]);

                temp[i].Index = CurrTextLength == 0 ? 0 : PrevTextIndex;
                temp[i].Length = CurrTextLength;

                PrevTextIndex += CurrTextLength;
            }

            for (int i = 0; i < fileDataCnt; i++)
            {
                fs.Seek(EntriseInStringInfo(this, i), SeekOrigin.Begin);

                bw.Write(temp[i].Index);
                bw.Write(temp[i].Length);
            }

            temp = null;

            int newLength = PrevTextIndex;
            int oldLength = (int)(fileLength - fileDataOff);

            fs.SetLength(fileLength + (newLength - oldLength));

            fs.Seek(fileDataOff, SeekOrigin.Begin);

            for (int i = 0; i < fileDataCnt; i++)
            {
                bw.Write(Encoder.GetBytes(mNewTexts[i]));
            }

            fileLength = fs.Length;
        }

        public byte[] GetEntry(int index)
        {
            fs.Seek(EntriseOffset(this, index), SeekOrigin.Begin);

            return br.ReadBytes(E_Size);
        }

        public void SetEntry(int index, byte[] buff)
        {
            fs.Seek(EntriseOffset(this, index), SeekOrigin.Begin);

            bw.Write(buff);
        }

        public void CreateBlankEntry(int index)
        {
            byte[] _BlankEntry = new byte[E_Size];

            ReplaceBytes(
                EntriseOffset(this, index),
                E_Size,
                _BlankEntry,
                _BlankEntry.Length);
        }

        public void RemoveItem(int index)
        {
            int strIndex, strLength = 0;

            GetStrPosition(index, out strIndex, out strLength);

            if (strIndex != 0 || strLength != 0)
                //remove text
                RemoveBytes((int)(fileDataOff + strIndex), strLength);

            //remove entry
            RemoveBytes(EntriseOffset(this, index), E_Size);
        }

        public string EntriseToString(int index, BitArray flags)
        {
            string result = string.Empty;

            var refstr = EntriseToString(index).Split(',');

            int refcnt = refstr.Length;

            for (int i = 0; i < refcnt; i++)
                if (flags[i]) result += refstr[i] + ',';

            return result.TrimEnd(',');
        }

        private byte[] GetHeaderBytes()
        {
            fs.Seek(0, SeekOrigin.Begin);

            return br.ReadBytes(H_Size);
        }

        private void PreLoadedStringTable()
        {
            StrPosTable = new (int, int)[fileDataCnt];

            int strIndex, strLength = 0;

            for (int i = 0; i < fileDataCnt; i++)
            {
                GetStrPosition(i, out strIndex, out strLength);

                StrPosTable[i].Item1 = strIndex;
                StrPosTable[i].Item2 = strLength;
            }
        }

        private bool IsChecked()
        {
            if (!File.Exists(filepath))
            {
                ErrorMsg = TLKFILE_ERR.NOTEXIST_FILE;
                return false;
            }

            if (IsFileLocked(filepath))
            {
                ErrorMsg = TLKFILE_ERR.NOT_ACCESS;
                return false;
            }

            if (!TLKFileVersionCheck())
            {
                ErrorMsg = TLKFILE_ERR.NOT_TLKFILE;
                return false;
            }

            return true;
        }

        private bool IsFileLocked(string filePath)
        {
            var fa = File.GetAttributes(filepath);

            if (fa == FileAttributes.ReadOnly) return true;

            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }

        private bool TLKFileVersionCheck()
        {
            bool result = false;

            if (filepath == string.Empty) return result;

            Version = GetFileVersion(filepath);

            switch (Version)
            {
                case BG_ChkStr: result = true; break;
                case NW_ChkStr: result = true; break;
                default: result = false; break;
            }

            return result;
        }

        private void GetStrPosition(int entriseNumber, out int strIndex, out int strLength)
        {
            fs.Seek(EntriseInStringInfo(this, entriseNumber), SeekOrigin.Begin);

            strIndex = br.ReadInt32();
            strLength = br.ReadInt32();
        }

        private void RemoveBytes(int offset, int length)
        {
            var Buff = new byte[(int)(fileLength - (offset + length))];
            int BuffLeng = Buff.Length;
            int readIndex = offset + length;
            int writeIndex = offset;

            fs.Seek(readIndex, SeekOrigin.Begin);
            br.Read(Buff, 0, BuffLeng);

            fs.SetLength(fileLength - length);

            fs.Seek(writeIndex, SeekOrigin.Begin);
            bw.Write(Buff, 0, BuffLeng);

            fileLength = fs.Length;
            Buff = null;
        }

        private void ReplaceBytes(int oldIndex, int oldLength, byte[] newbuf, int newLength)
        {
            var Buff = new byte[(fileLength - (oldIndex + oldLength))];
            int BuffLeng = Buff.Length;
            int readIndex = oldIndex + oldLength;

            fs.Seek(readIndex, SeekOrigin.Begin);
            br.Read(Buff, 0, BuffLeng);

            fs.SetLength(fileLength + (newLength - oldLength));

            fs.Seek(oldIndex, SeekOrigin.Begin);
            bw.Write(newbuf);
            bw.Write(Buff, 0, BuffLeng);
            
            fileLength = fs.Length;
            Buff = null;
        }

        public byte[] GetEntryResourceName(int index)
        {
            return GetEntryNoText(index).Skip(E_RsrcName).Take(E_RsrcNameLen).ToArray();
        }

        private void ModifyEntries(int entriseNumber, int textLength)
        {
            int count = 0, offset = 0;

            count = entriseNumber;
            offset = EntriseOffset(this, entriseNumber);

            var buff = new byte[fileDataOff - offset];
            int buffLeng = buff.Length;

            fs.Seek(offset, SeekOrigin.Begin);
            br.Read(buff, 0, buffLeng);

            int oldLength = BitConverter.ToInt32(buff, E_OffTosStrLen);
            int modifiedPoint = textLength - oldLength;

            var temp = BitConverter.GetBytes(textLength);
            Buffer.BlockCopy(temp, 0, buff, E_OffTosStrLen, sizeof(int));

            int buffIndex = E_OffToStr;
            int entrisesize = E_Size;
            int oldindex = 0;
            while (++count < fileDataCnt)
            {
                buffIndex += entrisesize;
                oldindex = BitConverter.ToInt32(buff, buffIndex);

                if (oldindex == 0 && BitConverter.ToInt32(buff, buffIndex + 4) == 0) continue;

                Buffer.BlockCopy(
                    BitConverter.GetBytes(oldindex + modifiedPoint), 0,
                    buff, buffIndex, sizeof(int));
            }

            fs.Seek(offset, SeekOrigin.Begin);
            bw.Write(buff, 0, buffLeng);

            buff = null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                    filepath = string.Empty;
                    fileDataCnt = 0;
                    fileDataOff = 0;
                    fileLength = 0;

                    if (br != null) { br.Dispose(); br.Close(); }
                    if (bw != null) { bw.Dispose(); bw.Close(); }
                    if (fs != null) { fs.Dispose(); fs.Close(); }

                    if (_PreLoad) StrPosTable = null;
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~TLKFILE() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            // GC.SuppressFinalize(this);
        }
        #endregion

        private static int EntriseOffset(TLKFILE tlk, int entriseNumber)
        {
            return tlk.H_Size + (entriseNumber * tlk.E_Size);
        }

        private static int EntriseInStringInfo(TLKFILE tlk, int entriseNumber)
        {
            return EntriseOffset(tlk, entriseNumber) + tlk.E_OffToStr;
        }

        private static string GetFileVersion(string filepath)
        {
            string result = string.Empty;

            using (var _fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                var _br = new BinaryReader(_fs);

                result = Encoding.ASCII.GetString(_br.ReadBytes(8));
            }

            return result;
        }

        public static TLKFILE Open(string path)
        {
            switch (GetFileVersion(path))
            {
                case BG_ChkStr: return new TLK_V1(path);
                case NW_ChkStr: return new TLK_V3(path);
                default: return null;
            }
        }

        public static TLKFILE Open(string path, bool preload)
        {
            switch (GetFileVersion(path))
            {
                case BG_ChkStr: return new TLK_V1(path, preload);
                case NW_ChkStr: return new TLK_V3(path, preload);
                default: return null;
            }
        }
    }

    public class TLK_V1 : TLKFILE
    {
        public override int H_Size => (int)HSIZE.headerSize;
        public override int H_OffToStrCnt => (int)HSIZE._stringCntOff;
        public override int E_Size => (int)ESIZE.entriseSize;
        public override int E_OffToStr => (int)ESIZE._stringOffsetOff;
        public override int E_OffTosStrLen => (int)ESIZE._stringLengthOff;
        public override int E_RsrcName => (int)ESIZE._resourceNameOff;
        public override int E_RsrcNameLen => (int)ESIZE._resourceNameSize;

        public override byte[] GetEntryNoText(int index)
        {
            var result = new byte[E_OffToStr];

            Buffer.BlockCopy(GetEntry(index), 0, result, 0, E_OffToStr);

            return result;
        }

        public override string EntriseToString(int index)
        {
            var temp = new Entries(GetEntryNoText(index));

            return $"{temp.Type}, {Encoder.GetString(temp.ResourceName)}, {temp.Volume}, {temp.Pitch}";
        }

        public override bool[] CompareEntrise(byte[] left, byte[] right)
        {
            var temp1 = new Entries(left);
            var temp2 = new Entries(right);

            bool[] result = new bool[5];

            result[0] = temp1.Type != temp2.Type;
            result[1] = !string.Equals(Encoder.GetString(temp1.ResourceName), Encoder.GetString(temp2.ResourceName));
            result[2] = temp1.Volume != temp2.Volume;
            result[3] = temp1.Pitch != temp2.Pitch;

            return result;
        }

        private enum HSIZE // TLK File HEAD Struct offset...
        {
            _signatureOff = 0,
            _signatureSize = 4,
            _versionOff = _signatureOff + _signatureSize, //4
            _versionSize = 4,
            _languageIDOff = _versionOff + _versionSize, //8
            _languageIDSize = 2,
            _stringCntOff = _languageIDOff + _languageIDSize, //10
            _stringCntSize = 4,
            _stringDataOff = _stringCntOff + _stringCntSize, //14
            _stringDataSize = 4,
            headerSize = _stringDataOff + _stringDataSize, //18
        }

        private enum ESIZE // TLK File Entries Struct offset...
        {
            _dataTypeOff = 0,
            _dataTypeSize = 2,
            _resourceNameOff = _dataTypeOff + _dataTypeSize, //2
            _resourceNameSize = 8,
            _volumeVarianceOff = _resourceNameOff + _resourceNameSize, //10
            _volumeVarianceSize = 4,
            _pitchVarianceOff = _volumeVarianceOff + _volumeVarianceSize, //14
            _pitchVarianceSize = 4,
            _stringOffsetOff = _pitchVarianceOff + _pitchVarianceSize, //18
            _stringOffsetSize = 4,
            _stringLengthOff = _stringOffsetOff + _stringOffsetSize, // 22
            _stringLengthSize = 4,
            entriseSize = _stringLengthOff + _stringLengthSize, // 26
        }

        public TLK_V1(string filepath) : base(filepath) { }

        public TLK_V1(string filepath, bool preload) : base(filepath, preload) { }

        private readonly struct Entries
        {
            public readonly short Type;
            public readonly byte[] ResourceName;
            public readonly int Volume;
            public readonly int Pitch;
            // public readonly int StrOffset;
            // public readonly int StrLength;

            public Entries(byte[] buff)
            {
                Type = BitConverter.ToInt16(buff, (int)ESIZE._dataTypeOff);

                ResourceName = new byte[(int)ESIZE._resourceNameSize];
                Buffer.BlockCopy(buff, (int)ESIZE._resourceNameOff, ResourceName, 0, (int)ESIZE._resourceNameSize);

                Volume = BitConverter.ToInt32(buff, (int)ESIZE._volumeVarianceOff);
                Pitch = BitConverter.ToInt32(buff, (int)ESIZE._pitchVarianceOff);
                // StrOffset = BitConverter.ToInt32(buff, (int)ESIZE._stringOffsetOff);
                // StrLength = BitConverter.ToInt32(buff, (int)ESIZE._stringLengthOff);
            }
        }
    }

    public class TLK_V3 : TLKFILE
    {
        public override int H_Size => (int)HSIZE.headerSize;
        public override int H_OffToStrCnt => (int)HSIZE._stringCntOff;
        public override int E_Size => (int)ESIZE.entriseSize;
        public override int E_OffToStr => (int)ESIZE._stringOffsetOff;
        public override int E_OffTosStrLen => (int)ESIZE._stringLengthOff;
        public override int E_RsrcName => (int)ESIZE._resourceNameOff;
        public override int E_RsrcNameLen => (int)ESIZE._resourceNameSize;

        public override byte[] GetEntryNoText(int index)
        {
            var src = GetEntry(index);
            var dst = new byte[E_OffToStr + (int)ESIZE._soundLengthSize];

            Buffer.BlockCopy(src, 0, dst, 0, E_OffToStr);

            Buffer.BlockCopy(src, (int)ESIZE._soundLengthOff, dst, E_OffToStr, (int)ESIZE._soundLengthSize);

            return dst;
        }

        public override string EntriseToString(int index)
        {
            var temp = new Entries(GetEntryNoText(index));

            return $"{temp.Type}, {Encoder.GetString(temp.ResourceName)}, {temp.Volume}, {temp.Pitch}, {temp.SndLength}";
        }

        public override bool[] CompareEntrise(byte[] left, byte[] right)
        {
            var temp1 = new Entries(left);
            var temp2 = new Entries(right);

            bool[] result = new bool[6];

            result[0] = temp1.Type != temp2.Type;
            result[1] = !string.Equals(Encoder.GetString(temp1.ResourceName), Encoder.GetString(temp2.ResourceName));
            result[2] = temp1.Volume != temp2.Volume;
            result[3] = temp1.Pitch != temp2.Pitch;
            result[4] = temp1.SndLength != temp2.SndLength;

            return result;
        }

        private enum HSIZE // TLK File HEAD Struct offset...
        {
            _signatureOff = 0,
            _signatureSize = 4,
            _versionOff = _signatureOff + _signatureSize, //4
            _versionSize = 4,
            _languageIDOff = _versionOff + _versionSize, //8
            _languageIDSize = 4,
            _stringCntOff = _languageIDOff + _languageIDSize, //12
            _stringCntSize = 4,
            _stringDataOff = _stringCntOff + _stringCntSize, //16
            _stringDataSize = 4,
            headerSize = _stringDataOff + _stringDataSize, //20
        }

        private enum ESIZE // TLK File Entries Struct offset...
        {
            _dataTypeOff = 0,
            _dataTypeSize = 4,
            _resourceNameOff = _dataTypeOff + _dataTypeSize, //4
            _resourceNameSize = 16,
            _volumeVarianceOff = _resourceNameOff + _resourceNameSize, //20
            _volumeVarianceSize = 4,
            _pitchVarianceOff = _volumeVarianceOff + _volumeVarianceSize, //24
            _pitchVarianceSize = 4,
            _stringOffsetOff = _pitchVarianceOff + _pitchVarianceSize, //28
            _stringOffsetSize = 4,
            _stringLengthOff = _stringOffsetOff + _stringOffsetSize, //32
            _stringLengthSize = 4,
            _soundLengthOff = _stringLengthOff + _stringLengthSize, //36
            _soundLengthSize = 4,
            entriseSize = _soundLengthOff + _soundLengthSize,  //40
        }

        public TLK_V3(string filepath) : base(filepath) { }

        public TLK_V3(string filepath, bool preload) : base(filepath, preload) { }

        private readonly struct Entries
        {
            public readonly int Type;
            public readonly byte[] ResourceName;
            public readonly int Volume;
            public readonly int Pitch;
            //public readonly int StrOffset;
            //public readonly int StrLength;
            public readonly int SndLength;

            public Entries(byte[] buff)
            {
                Type = BitConverter.ToInt32(buff, (int)ESIZE._dataTypeOff);

                ResourceName = new byte[(int)ESIZE._resourceNameSize];
                Buffer.BlockCopy(buff, (int)ESIZE._resourceNameOff, ResourceName, 0, (int)ESIZE._resourceNameSize);

                Volume = BitConverter.ToInt32(buff, (int)ESIZE._volumeVarianceOff);
                Pitch = BitConverter.ToInt32(buff, (int)ESIZE._pitchVarianceOff);
                //StrOffset = BitConverter.ToInt32(buff, (int)ESIZE._stringOffsetOff);
                //StrLength = BitConverter.ToInt32(buff, (int)ESIZE._stringLengthOff);
                SndLength = BitConverter.ToInt32(buff, (int)ESIZE._stringOffsetOff);
            }
        }
    }
}
