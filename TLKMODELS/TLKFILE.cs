using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TLKMODELS.IO
{
    public class TLKFILE : IDisposable
    {
        #region Fields

        private bool _PreLoad = false;

        private const string CheckString = "TLK V1  ";

        private string filepath { get; set; }
        private FileStream fs { get; set; } = null;
        private BinaryReader br { get; set; } = null;
        private BinaryWriter bw { get; set; } = null;

        private (int,int)[] StrPosTable { get; set; }

        public Encoding Encoder { get; set; } = Encoding.UTF8; // Encoding.GetEncoding("euc-kr");
        public uint fileDataCnt { get; private set; }
        public uint fileDataOff { get; private set; }
        public long fileLength { get; private set; }

        public string ErrorMsg { get; set; }

        #endregion

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

            fs.Seek((int)HSIZE._stringCntOff, SeekOrigin.Begin);
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

            ReplaceBytes(oldIndex, oldLength, Encoder.GetBytes(text), textlength);

            ModifyEntries(Index, textlength);

            fileLength = fs.Length;
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

        private byte[] GetEntry(int index)
        {
            fs.Seek(EntriseOffset(index), SeekOrigin.Begin);

            return br.ReadBytes((int)ESIZE.entriseSize);
        }

        private void SetEntry(int index, byte[] buff)
        {
            fs.Seek(EntriseOffset(index), SeekOrigin.Begin);

            bw.Write(buff);
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

            using (var _fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                var _br = new BinaryReader(_fs);

                result = string.Equals(
                    string.Format("{0}{1}",
                    Encoding.ASCII.GetString(_br.ReadBytes((int)HSIZE._signatureSize)),
                    Encoding.ASCII.GetString(_br.ReadBytes((int)HSIZE._versionSize))),
                    CheckString);
            }

            return result;
        }

        private void GetStrPosition(int entriseNumber, out int strIndex, out int strLength)
        {
            fs.Seek(EntriseInStringInfo(entriseNumber), SeekOrigin.Begin);

            strIndex = br.ReadInt32();
            strLength = br.ReadInt32();
        }

        private void ReplaceBytes(int oldIndex, int oldLength, byte[] text, int textlength)
        {
            var Buff = new byte[(int)(fileLength - (fileDataOff + oldIndex + oldLength))];
            int BuffLeng = Buff.Length;
            int readIndex = (int)fileDataOff + oldIndex + oldLength;
            int writeIndex = (int)fileDataOff + oldIndex + textlength;

            fs.Seek(readIndex, SeekOrigin.Begin);
            br.Read(Buff, 0, BuffLeng);

            fs.SetLength(fileLength + (textlength - oldLength));

            fs.Seek(writeIndex, SeekOrigin.Begin);
            bw.Write(Buff, 0, BuffLeng);

            fs.Seek(fileDataOff + oldIndex, SeekOrigin.Begin);
            bw.Write(text);
        }

        private void ModifyEntries(int entriseNumber, int textLength)
        {
            int count = 0, offset = 0;

            count = entriseNumber;
            offset = EntriseOffset(entriseNumber);

            var buff = new byte[fileDataOff - offset];
            int buffLeng = buff.Length;

            fs.Seek(offset, SeekOrigin.Begin);
            br.Read(buff, 0, buffLeng);

            int oldLength = BitConverter.ToInt32(buff, 22);
            int modifiedPoint = textLength - oldLength;

            Buffer.BlockCopy(
                BitConverter.GetBytes(textLength), 0,
                buff, (int)ESIZE._stringLengthOff, sizeof(int));

            int buffIndex = (int)ESIZE._stringOffsetOff;
            int oldindex = 0;
            while (++count < fileDataCnt)
            {
                buffIndex += (int)ESIZE.entriseSize;
                oldindex = BitConverter.ToInt32(buff, buffIndex);

                if (oldindex != 0)
                {
                    Buffer.BlockCopy(
                        BitConverter.GetBytes(oldindex + modifiedPoint), 0,
                        buff, buffIndex, sizeof(int));
                }
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

        #region TLK File Offset Macro

        private static int EntriseOffset(int entriseNumber)
        {
            return (int)HSIZE.headerSize + (entriseNumber * (int)ESIZE.entriseSize);
        }

        private static int EntriseInStringInfo(int entriseNumber)
        {
            return EntriseOffset(entriseNumber) + (int)ESIZE._stringOffsetOff;
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

        #endregion
    }

    internal sealed class TLKFILE_ERR
    {
        public const string NOTHING_ERR = "";
        public const string NOT_ACCESS = "해당 FILE 은 이미 사용 중입니다.";
        public const string NOT_TLKFILE = "TLK FILE 이 아닙니다.";
        public const string NOTEXIST_FILE = "FILE 이 없습니다.";
    }
}
