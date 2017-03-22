## Rategain

### 项目背景:
     依据第三方网站给出的价格与平台CMS价格做RateGain比较，需要每日定时SFTP下载csv文件， 并将内容导入到redis数据库。
### 功能点：
  + 日志记录
  + SFTP下载
  + Redis 特性
  + 酒店名称使用 Lucene.net匹配 
### 技术点： 
  + 利用 PostSharp 做AOP编程：日志记录 ---现已经移除
  + Renci.SshNet 做SFTP 文件下载，其中TPL模型将下载csv和导入redis 做成了级联任务  
  + 因为下载的csv文件酒店名称经常与平台CMS的 hotelname 不一致，这在酒店比较多的情况下,人工匹配很费时费力，故采用Lucene.net 做搜索匹配
  + 利用redis 发布/订阅特性对未在CMS hotel name 集合的新加入hotel 做了处理
      
### 总结：
  + 本项目自认为比较好的地方是 采用Task做了级联任务（将redis导入功能作为级联任务)
  + 利用redis发布/订阅特性处理新加入的酒店，同时应用了Lucene.net做搜索匹配
  + 项目实现了完整的日志功能类，Redis数据库管理类，SFTP批量文件下载类
  + 同时针对批量下载-导入数据的模型 提供两种行为： 单文件下载完成后处理，批量下载完成后批量处理
  + 在此感谢nuget上面第三方dll提供者提供的功能组件。

       
