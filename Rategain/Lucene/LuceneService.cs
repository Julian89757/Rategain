using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using RateGain.Util;
using Version = Lucene.Net.Util.Version;

namespace RateGainData.Console
{
    public interface ILuceneService
    {
        void BuildIndex(IEnumerable<SampleDataFileRow> dataToIndex);
        IEnumerable<SampleDataFileRow> Search(string searchTerm);
        void CloseDirectory();
    }

    public class LuceneService : ILuceneService
    {
        // Note there are many different types of Analyzer that may be used with Lucene, the exact one you use
        // will depend on your requirements
        private Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);    
        private Directory  luceneIndexDirectory;
        private IndexWriter writer;
        private string indexPath = @"c:\temp\LuceneIndex";

        public LuceneService()
        {
            InitialiseLucene();
        }

        private void InitialiseLucene()
        {
            if(System.IO.Directory.Exists(indexPath))
            {
                try
                {
                    System.IO.Directory.Delete(indexPath, true);
                }
                catch (UnauthorizedAccessException ex)
                {
                    LogHelper.Write("Maybe there is no Permission to delete the dir files",LogHelper.LogMessageType.Warn);
                }
                
            }
            luceneIndexDirectory = FSDirectory.Open(indexPath);
            writer = new IndexWriter(luceneIndexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        //  create  a  new Lucene document  for  source  object
        public void BuildIndex(IEnumerable<SampleDataFileRow> dataToIndex)
        {
            foreach (var sampleDataFileRow in dataToIndex)
	        {
		        var doc = new Document();
                doc.Add(new Field("LineNumber", sampleDataFileRow.LineNumber.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
                doc.Add(new Field("LineText", sampleDataFileRow.LineText, Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(doc);
	        }
            writer.Optimize();
            writer.Flush(true,false,true);
            writer.Dispose();
            
        }

        public IEnumerable<SampleDataFileRow> Search(string searchTerm)
        {
            IndexSearcher searcher = new IndexSearcher(luceneIndexDirectory);
            QueryParser parser = new QueryParser(Version.LUCENE_30,"LineText", analyzer);

            Query query = parser.Parse(searchTerm);
            
            // 找到最可能匹配的三个, 可以直接返回最匹配的一条
            var hitsFound = searcher.Search(query, null,1);

            var results = new List<SampleDataFileRow>();

            if (!hitsFound.ScoreDocs.Any())
                return null;

            for (int i = 0; i < hitsFound.ScoreDocs.Count(); i++)
            {
                var tempDataFileRow = new SampleDataFileRow();

                var doc = searcher.Doc(hitsFound.ScoreDocs[i].Doc);

                tempDataFileRow.LineNumber = int.Parse(doc.Get("LineNumber"));
                tempDataFileRow.LineText = doc.Get("LineText");
                tempDataFileRow.Score = hitsFound.ScoreDocs[i].Score;

                results.Add(tempDataFileRow);
            }
            return new List<SampleDataFileRow> {results.OrderByDescending(x => x.Score).FirstOrDefault() };
        }

        public void CloseDirectory()
        {
            luceneIndexDirectory.Dispose();
        }
    }
}


/*
 * 根据实际需求 选择合适分词算法解析器
 * 利用分词算法解析器 初始化索引写入器
 * 用（上一步的索引写入器）将原对象（这里是每行数据）以lucene文档的形式写入索引目录
 * 在索引 （indexSearcher）中查询（QueryParser）指定的Term，注意，使用什么分词算法解析器，就要使用什么解析器作为queryParser 参数
 * 
 * 
 分词搜索是算法的核心，所有分词算法都是从Analyzer 类继承：
 *内置的StandardAnalyzer 是将英文按照空格，标点符号等进行分词，将中文按照单个字进行分词
 *二元分词算法，每两个汉字算一个单词
 *基于词库的分词算法，网上有开源的 盘古分词，和庖丁解牛分词算法，值需要引用dll
 
 
 */