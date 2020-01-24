using System.Text;


namespace UTJ.FrameDebugSave
{
    public class CsvStringGenerator
    {
        private StringBuilder stringBuilder;
        public CsvStringGenerator()
        {
            stringBuilder = new StringBuilder(1024 * 1024);
        }
        public CsvStringGenerator(StringBuilder sb)
        {
            stringBuilder = sb;
        }
        public CsvStringGenerator AppendColumn(string val)
        {
            val = val.Replace(',', '.').Replace('\n', ' ');
            stringBuilder.Append(val).Append(',');
            return this;
        }
        public CsvStringGenerator AppendColumn(int val)
        {
            stringBuilder.Append(val).Append(',');
            return this;
        }
        public CsvStringGenerator AppendColumn(bool val)
        {
            stringBuilder.Append(val).Append(',');
            return this;
        }
        public CsvStringGenerator AppendColumn(long val)
        {
            stringBuilder.Append(val).Append(',');
            return this;
        }
        public CsvStringGenerator AppendColumn(ulong val)
        {
            stringBuilder.Append(val).Append(',');
            return this;
        }
        public CsvStringGenerator AppendColumn(float val)
        {
            stringBuilder.Append(val).Append(',');
            return this;
        }
        public CsvStringGenerator NextRow()
        {
            stringBuilder.Append("\n");
            return this;
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}