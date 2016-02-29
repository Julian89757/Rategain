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
      因为下载的csv文件酒店名称经常与平台CMS的 hotelname 不一致，这酒店比较多的情况下，人工匹配很费时费力，故采用 Lucene.net 
       
