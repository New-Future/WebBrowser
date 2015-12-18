# WebBrowser

基于TCP实现的简易浏览器，支持HTTPS链接和图片解析


## 目录结构
程序分为 四个子项目

>
```
│  WebBrowser.sln 解决方案
├─APITest API测试子项目目录
│  │  APITest.csproj
│  │  APIWindow.xaml
│  │  APIWindow.xaml.cs
│  │  App.config
│  │  App.xaml
│  │  App.xaml.cs
│  └─ favicon.ico
│          
├─HtmltoXaml HTML解析子项目目录
│  │  HtmltoXaml.csproj
│  │  RichTextBox.cs
│  │          
│  ├─Formatter
│  │      HtmlFormatter.cs
│  │      ITextFormatter.cs
│  │      PlainTextFormatter.cs
│  │      RtfFormatter.cs
│  │      XamlFormatter.cs
│  │      
│  ├─Html
│  │      htmlcssparser.cs
│  │      htmlfromxamlconverter.cs
│  │      htmllexicalanalyzer.cs
│  │      htmlparser.cs
│  │      htmlschema.cs
│  │      htmltokentype.cs
│  │      htmltoxamlconverter.cs
│  │      
│  └─RichTextBox
│
├─HTTP HTTP项目目录
│  │  HTTP.cs
│  └─Response.cs
│  
│                  
└─WebBrowser 浏览器GUI实现目录
    │  App.config
    │  App.xaml
    │  App.xaml.cs
    │  Download.cs
    │  HtmlPage.xaml
    │  HtmlPage.xaml.cs
    │  logo.png
    │  MainWindow.xaml
    │  MainWindow.xaml.cs
    │  packages.config
    │  SourceWindow.xaml
    │  SourceWindow.xaml.cs
    │  textView.xaml
    │  textView.xaml.cs
    └─ WebBrowser.csproj
```
>>