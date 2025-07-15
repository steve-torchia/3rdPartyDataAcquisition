using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Serialization
{
    /// <summary>
    /// Represents a partial class DP.Base.Serialization.SimpleBinarySerializer.
    /// </summary>
    public partial class SimpleBinarySerializer : IDisposableEx
    {
        private const short DefaultFileVersion = 2;
        private const short DefaultHeaderVersion = 0;
        private const byte NewHeader = 0;
        private const byte RepeatHeader = 1;

        private List<IColumn> columns;
        private Stream stream;
        private Func<List<IColumn>, object> createRowFunction;
        private Func<string, List<Tuple<string, Type>>, List<IColumn>> createColumnsFunction;
        private string dataBlockName;
        private long newFileMinSize = 0;
        private long localPosition = 0;

        private string filename;
        private int colCount;

        public SimpleBinarySerializer()
        {
        }

        public SimpleBinarySerializer(string fileName, long? initialFileSize)
        {
            this.filename = fileName;
            this.InitializeWriteStream(new FileStream(this.filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read), initialFileSize, true);
        }

        /// <summary>
        /// write a new binary file.
        /// </summary>
        /// <param name="filename">file name.</param>
        public SimpleBinarySerializer(Stream stream, long? initialFileSize)
        {
            this.InitializeWriteStream(stream, initialFileSize, false);
        }

        public SimpleBinarySerializer(Stream stream, long? initialFileSize, bool fileExist)
        {
            this.InitializeWriteStream(stream, initialFileSize, false, fileExist);
        }

        private void InitializeWriteStream(Stream stream, long? initialFileSize, bool autoCloseStream)
        {
            this.stream = stream;
            this.AutoCloseStream = autoCloseStream;
            if (this.stream.Length == 0)
            {
                byte[] versionBytes = BitConverter.GetBytes(DefaultFileVersion);
                stream.Write(versionBytes, 0, versionBytes.Length);

                //after the first byte for the version put in the filesize 
                var sizeBytes = BitConverter.GetBytes(0L);
                stream.Write(sizeBytes, 0, sizeBytes.Length);
                if (initialFileSize.HasValue)
                {
                    this.newFileMinSize = initialFileSize.Value;
                }
            }
            else
            {
                this.ReadFileHeader();
                this.stream.Position = this.DataSize;
            }
        }

        private void InitializeWriteStream(Stream stream, long? initialFileSize, bool autoCloseStream, bool fileExist)
        {
            this.stream = stream;
            this.AutoCloseStream = autoCloseStream;
            if (!fileExist)
            {
                byte[] versionBytes = BitConverter.GetBytes(DefaultFileVersion);
                stream.Write(versionBytes, 0, versionBytes.Length);

                //after the first byte for the version put in the filesize 
                var sizeBytes = BitConverter.GetBytes(0L);
                stream.Write(sizeBytes, 0, sizeBytes.Length);
                if (initialFileSize.HasValue)
                {
                    this.newFileMinSize = initialFileSize.Value;
                }
            }
            else
            {
                //this.ReadFileHeader();
                this.stream.Position = 0;
            }
        }

        public SimpleBinarySerializer(Stream stream,
                Func<string, List<Tuple<string, Type>>, List<IColumn>> createColumnsFunction,
                Func<List<IColumn>, object> createRowFunction)
        {
            this.InitializeReadStream(stream, createColumnsFunction, createRowFunction, false);
        }

        /// <summary>
        /// read from a binary file.
        /// </summary>
        /// <param name="filename">file name.</param>
        /// <param name="createColFunction">create col function.</param>
        public SimpleBinarySerializer(
            string fileName,
            Func<string, List<Tuple<string, Type>>, List<IColumn>> createColumnsFunction,
            Func<List<IColumn>, object> createRowFunction)
        {
            this.filename = fileName;
            var stream = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            this.InitializeReadStream(stream, createColumnsFunction, createRowFunction, true);
        }

        public SimpleBinarySerializer(Stream stream, Func<string, List<Tuple<string, Type>>, List<IColumn>> createColumnsFunction, Func<List<IColumn>, object> createRowFunction, long dataSize)
        {
            this.InitializeReadStream(stream, createColumnsFunction, createRowFunction, false, dataSize);
        }

        private void InitializeReadStream(Stream stream, Func<string, List<Tuple<string, Type>>, List<IColumn>> createColumnsFunction, Func<List<IColumn>, object> createRowFunction, bool autoCloseStream, long dataSize)
        {
            this.AutoCloseStream = autoCloseStream;
            this.stream = stream;
            this.createRowFunction = createRowFunction;
            this.createColumnsFunction = createColumnsFunction;

            this.ReadFileHeader(dataSize);
        }

        private void InitializeReadStream(
            Stream stream, 
            Func<string, List<Tuple<string, Type>>, List<IColumn>> createColumnsFunction, 
            Func<List<IColumn>, object> createRowFunction,
            bool autoCloseStream)
        {
            this.AutoCloseStream = autoCloseStream;
            this.stream = stream;
            this.createRowFunction = createRowFunction;
            this.createColumnsFunction = createColumnsFunction;

            this.ReadFileHeader();
        }

        public void AddHeader(string dataBlockName, List<IColumn> cols, int rowCount)
        {
            this.dataBlockName = dataBlockName;
            this.SetColumns(cols);

            byte[] versionBytes = BitConverter.GetBytes(DefaultHeaderVersion);
            this.stream.Write(versionBytes, 0, versionBytes.Length);

            this.stream.WriteByte(SimpleBinarySerializer.NewHeader);

            var tmpBuf = BitConverter.GetBytes(rowCount);
            this.stream.Write(tmpBuf, 0, tmpBuf.Length);

            //datablockname size
            var dataBlockNameBuf = Encoding.ASCII.GetBytes(this.dataBlockName);
            tmpBuf = BitConverter.GetBytes(dataBlockNameBuf.Length);
            this.stream.Write(tmpBuf, 0, tmpBuf.Length);
            this.stream.Write(dataBlockNameBuf, 0, dataBlockNameBuf.Length);

            tmpBuf = BitConverter.GetBytes(cols.Count);
            this.stream.Write(tmpBuf, 0, tmpBuf.Length);

            foreach (var col in this.columns)
            {
                //col type
                tmpBuf = BitConverter.GetBytes(this.GetTypeCode(col.GetColumnType()));
                this.stream.Write(tmpBuf, 0, tmpBuf.Length);

                //get encoded name
                var colNameBuf = Encoding.ASCII.GetBytes(col.Name);

                //col name size
                tmpBuf = BitConverter.GetBytes(colNameBuf.Length);
                this.stream.Write(tmpBuf, 0, tmpBuf.Length);

                //col name
                this.stream.Write(colNameBuf, 0, colNameBuf.Length);
            }
        }

        public long DataSize { get; set; }

        public short FileVersion { get; private set; }

        public void SetColumns(List<IColumn> cols)
        {
            this.columns = cols.OrderBy(c => c.Index).ToList();
        }

        public void AddRepeatHeader(int rowCount)
        {
            byte[] versionBytes = BitConverter.GetBytes(DefaultHeaderVersion);
            this.stream.Write(versionBytes, 0, versionBytes.Length);

            this.stream.WriteByte(SimpleBinarySerializer.RepeatHeader);

            var tmpBuf = BitConverter.GetBytes(rowCount);
            this.stream.Write(tmpBuf, 0, tmpBuf.Length);
        }

        public void WriteRow(object row)
        {
            foreach (var c in this.columns)
            {
                c.WriteData(row, this.stream);
            }
        }

        private bool closed;
        public void CloseFile()
        {
            var localStream = this.stream;

            if (this.closed == true ||
                localStream == null)
            {
                return;
            }

            this.stream = null;
            this.closed = true;

            if (localStream.CanWrite)
            {
                this.DataSize = localStream.Position;
                var newDataSizeBytes = BitConverter.GetBytes(this.DataSize);

                localStream.Position = 2;

                localStream.Write(newDataSizeBytes, 0, newDataSizeBytes.Length);

                if (this.newFileMinSize != 0 &&
                    localStream.Length < this.newFileMinSize)
                {
                    localStream.Position = this.newFileMinSize;
                    localStream.WriteByte(0);
                }

                localStream.Flush();
            }

            if (this.AutoCloseStream == true)
            {
                localStream.Dispose();
            }
        }

        public bool ReadNextBlock(List<object> objectList)
        {
            if (this.localPosition >= this.DataSize)
            {
                return false;
            }

            var buffer = new byte[1000];

            //read the header
            //read header version
            SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 2);

            this.localPosition += 2;
            var hVersion = BitConverter.ToInt16(buffer, 0);

            //read header type
            SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 1);

            this.localPosition += 1;
            var hType = buffer[0];

            SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 4);
            this.localPosition += 4;
            var rowCount = BitConverter.ToInt32(buffer, 0);

            if (hType == NewHeader)
            {
                //datablockname size
                SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 4);
                this.localPosition += 4;

                int dataBlockNameBufSize = BitConverter.ToInt32(buffer, 0);
                SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, dataBlockNameBufSize);
                this.localPosition += dataBlockNameBufSize;

                this.dataBlockName = Encoding.ASCII.GetString(buffer, 0, dataBlockNameBufSize);

                SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 4);
                this.localPosition += 4;
                this.colCount = BitConverter.ToInt32(buffer, 0);

                var colDescs = new List<Tuple<string, Type>>();
                //create the colums for the header
                //read the columns
                for (int colIndex = 0; colIndex < this.colCount; colIndex++)
                {
                    //col type
                    SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 2);
                    this.localPosition += 2;
                    var type = this.GetTypeFromTypeCode(BitConverter.ToInt16(buffer, 0));

                    //get col name
                    SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 4);
                    this.localPosition += 4;
                    var nameSize = BitConverter.ToInt32(buffer, 0);

                    SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, nameSize);
                    this.localPosition += nameSize;
                    var colName = Encoding.ASCII.GetString(buffer, 0, nameSize);

                    colDescs.Add(new Tuple<string, Type>(colName, type));
                }

                this.columns = this.createColumnsFunction(this.dataBlockName, colDescs);
            }
            else if (hType == RepeatHeader)
            {
                //nothing to do
            }

            //read each row for the header
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                if (this.localPosition >= this.DataSize)
                {
                    return true;
                }

                var row = this.createRowFunction(this.columns);
                if (row == null)
                {
                    continue;
                }

                try
                {
                    this.ReadRow(row, buffer);
                }
                catch (Exception)
                {
                    return false;
                }

                if (objectList != null)
                {
                    objectList.Add(row);
                }
            }

            return true;
        }

        private void ReadFileHeader()
        {
            byte[] buffer = new byte[8];

            SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 2);
            this.localPosition += 2;

            this.FileVersion = BitConverter.ToInt16(buffer, 0);

            SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 8);
            this.localPosition += 8;

            this.DataSize = BitConverter.ToInt64(buffer, 0);
        }

        private void ReadFileHeader(long dataSize)
        {
            byte[] buffer = new byte[8];

            SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 2);
            this.localPosition += 2;

            this.FileVersion = BitConverter.ToInt16(buffer, 0);

            SimpleBinarySerializer.ReadBytesToBuffer(this.stream, buffer, 8);
            this.localPosition += 8;

            this.DataSize = dataSize;
        }

        private void ReadRow(object row, byte[] sharedBuffer)
        {
            for (int colIndex = 0; colIndex < this.colCount; colIndex++)
            {
                var c = this.columns[colIndex];
                if (c == null)
                {
                    continue;
                }

                long tmpCount = 0;
                c.ReadData(row, this.stream, sharedBuffer, out tmpCount);
                this.localPosition += tmpCount;
            }
        }

        private short GetTypeCode(Type type)
        {
            if (type == typeof(float))
            {
                return (short)TypeCode.Single;
            }
            else if (type == typeof(double))
            {
                return (short)TypeCode.Double;
            }
            else if (type == typeof(int))
            {
                return (short)TypeCode.Int32;
            }
            else if (type == typeof(string))
            {
                return (short)TypeCode.String;
            }

            throw new InvalidDataException("no support for type:" + type.ToString());
        }

        private Type GetTypeFromTypeCode(short typeCode)
        {
            if (typeCode == (short)TypeCode.Single)
            {
                return typeof(float);
            }
            else if (typeCode == (short)TypeCode.Double)
            {
                return typeof(double);
            }
            else if (typeCode == (short)TypeCode.Int32)
            {
                return typeof(int);
            }
            else if (typeCode == (short)TypeCode.String)
            {
                return typeof(string);
            }

            throw new InvalidDataException("no support for type:" + typeCode.ToString());
        }

        private static void ReadBytesToBuffer(Stream stream, byte[] buffer, int numberToRead)
        {
            int readCount = 0;
            int remaining = numberToRead;
            while (remaining > 0)
            {
                int tmpReadCount = stream.Read(buffer, readCount, remaining);
                remaining -= tmpReadCount;
                readCount += tmpReadCount;
            }
        }

        #region IDisposable
        public bool IsDisposed { get; private set; }

        public event EventHandler Disposing;

        public void Dispose()
        {
            this.Dispose(true);

            // Use SupressFinalize in case a subclass of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            try
            {
                if (this.stream != null)
                {
                    this.CloseFile();
                }

                if (this.Disposing != null)
                {
                    this.Disposing(this, EventArgs.Empty);
                }
            }
            catch
            {
                //dont care?
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~SimpleBinarySerializer()
        {
            this.Dispose(false);
        }
        #endregion

        public abstract class RowColumnDataGetter<T>
        {
            public abstract T GetData(object row);

            public abstract void SetData(object row, T value);
        }

        public class SimpleRowColumnDataGetter<T, TO> : RowColumnDataGetter<T>
            where TO : class
        {
            private Func<TO, T> getFunction;
            private Action<TO, T> setAction;

            public SimpleRowColumnDataGetter(Func<TO, T> getFunction, Action<TO, T> setAction)
            {
                this.getFunction = getFunction;
                this.setAction = setAction;
            }

            public override T GetData(object row)
            {
                return this.getFunction((TO)row);
            }

            public override void SetData(object row, T value)
            {
                this.setAction((TO)row, value);
            }
        }

        public interface IColumn
        {
            void WriteData(object row, Stream s);

            void ReadData(object row, Stream s, byte[] sharedBuffer, out long numberOfBytesRead);

            Type GetColumnType();

            string Name { get; }
            int Index { get; }
        }

        public abstract class Column<T> : IColumn
        {
            protected Column(string name, int index, RowColumnDataGetter<T> getter)
            {
                this.getter = getter;
                this.Name = name;
                this.Index = index;
            }

            protected RowColumnDataGetter<T> getter;
            public string Name { get; set; }

            public int Index { get; set; }

            public abstract void WriteData(object row, Stream s);

            public abstract void ReadData(object row, Stream s, byte[] sharedBuffer, out long numberOfBytesRead);

            public Type GetColumnType()
            {
                return typeof(T);
            }
        }

        public class DoubleColumn : Column<double>
        {
            public DoubleColumn(string name, int index, RowColumnDataGetter<double> getter)
                : base(name, index, getter)
            {
            }

            public override void WriteData(object row, Stream s)
            {
                var doubleBytes = BitConverter.GetBytes(this.getter.GetData(row));

                s.Write(doubleBytes, 0, doubleBytes.Length);
            }

            public override void ReadData(object row, Stream s, byte[] sharedBuffer, out long numberOfBytesRead)
            {
                if (sharedBuffer == null)
                {
                    sharedBuffer = new byte[8];
                }

                SimpleBinarySerializer.ReadBytesToBuffer(s, sharedBuffer, 8);

                numberOfBytesRead = 8;

                this.getter.SetData(row, BitConverter.ToDouble(sharedBuffer, 0));
            }
        }

        public class SingleColumn : Column<float>
        {
            public SingleColumn(string name, int index, RowColumnDataGetter<float> getter)
                : base(name, index, getter)
            {
            }

            public override void WriteData(object row, Stream s)
            {
                var singleBytes = BitConverter.GetBytes(this.getter.GetData(row));

                s.Write(singleBytes, 0, singleBytes.Length);
            }

            public override void ReadData(object row, Stream s, byte[] sharedBuffer, out long numberOfBytesRead)
            {
                if (sharedBuffer == null)
                {
                    sharedBuffer = new byte[4];
                }

                SimpleBinarySerializer.ReadBytesToBuffer(s, sharedBuffer, 4);

                numberOfBytesRead = 4;

                this.getter.SetData(row, BitConverter.ToSingle(sharedBuffer, 0));
            }
        }

        public class IntColumn : Column<int>
        {
            public IntColumn(string name, int index, RowColumnDataGetter<int> getter)
                : base(name, index, getter)
            {
            }

            public override void WriteData(object row, Stream s)
            {
                var intBytes = BitConverter.GetBytes(this.getter.GetData(row));

                s.Write(intBytes, 0, intBytes.Length);
            }

            public override void ReadData(object row, Stream s, byte[] sharedBuffer, out long numberOfBytesRead)
            {
                if (sharedBuffer == null)
                {
                    sharedBuffer = new byte[4];
                }

                SimpleBinarySerializer.ReadBytesToBuffer(s, sharedBuffer, 4);

                numberOfBytesRead = 4;

                this.getter.SetData(row, BitConverter.ToInt32(sharedBuffer, 0));
            }
        }

        public class AsciiColumn : Column<string>
        {
            public AsciiColumn(string name, int index, RowColumnDataGetter<string> getter)
                : base(name, index, getter)
            {
            }

            public override void WriteData(object row, Stream s)
            {
                var asciiBytes = Encoding.ASCII.GetBytes(this.getter.GetData(row));

                var sizeBytes = BitConverter.GetBytes(asciiBytes.Length);
                if (asciiBytes.Length > 100000000)
                {
                    throw new InvalidOperationException("Column size cannot be bigger than 100,000,000");
                }

                s.Write(sizeBytes, 0, sizeBytes.Length);
                s.Write(asciiBytes, 0, asciiBytes.Length);
            }

            public override void ReadData(object row, Stream s, byte[] sharedBuffer, out long numberOfBytesRead)
            {
                if (sharedBuffer == null)
                {
                    sharedBuffer = new byte[4];
                }

                SimpleBinarySerializer.ReadBytesToBuffer(s, sharedBuffer, 4);

                int size = BitConverter.ToInt32(sharedBuffer, 0);
                if (size > 100000000)
                {
                    throw new InvalidOperationException("Column size cannot be bigger than 100,000,000");
                }

                if (sharedBuffer.Length < size)
                {
                    sharedBuffer = new byte[size];
                }

                SimpleBinarySerializer.ReadBytesToBuffer(s, sharedBuffer, size);

                numberOfBytesRead = 4 + size;
                this.getter.SetData(row, Encoding.ASCII.GetString(sharedBuffer, 0, size));
            }
        }

        public bool AutoCloseStream { get; set; }
    }
}
