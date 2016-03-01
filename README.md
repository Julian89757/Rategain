# Rategain.Console

#项目背景:
      依据第三方网站给出的价格与平台CMS价格做 RateGain比较，需要每日定时使用SFTP下载csv文件， 并将内容导入到redis数据库。
#功能点：
      日志记录
      SFTP下载
      Redis 操作
      酒店名称 Lucene.net匹配 
#技术点： 
      利用 PostSharp 做AOP编程：日志记录
      Renci.SshNet 做SFTP 文件下载，其中Task异步编程模型将下载csv和导入redis 做成了级联任务。  
      因为下载的csv文件酒店名称经常与平台CMS的 hotelname 不一致，这酒店比较多的情况下，    
      人工匹配很费时费力，故采用Lucene.net 做搜索匹配。  
      
#总结：
      本项目自认为比较好的地方是 采用Task做了级联任务（将redis导入功能【委托】作为级联子任务），    
      AOP织入Log，同时应用了Lucene.net做搜索匹配。
      项目中 LoghHelper RedisCollection/RedisCacheManager，SFTPOperation 都是完整独立的文件，可以自由应用在项目中。
      
      在此感谢nuget上面第三方dll提供者提供的功能组件。

       
