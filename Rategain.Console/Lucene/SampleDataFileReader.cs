using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RateGain.Console
{
    public interface ISampleDataFileReader
    {
        IEnumerable<SampleDataFileRow> ReadAllRows();
    }

    public class SampleDataFileReader : ISampleDataFileReader
    {
        // 读取所有行
        public IEnumerable<SampleDataFileRow> ReadAllRows()
        {
            FileInfo assFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string file = string.Format(@"{0}\Lucene\SampleDataFile.txt", assFile.Directory.FullName);
            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
		    {
                yield return new SampleDataFileRow
                {
                    LineNumber = i + 1,
                    LineText = lines[i]
                };
		    }     
        }
    }
}
