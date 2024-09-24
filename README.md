


tool build

```bash
dotnet pack -c Release /p:PackAsTool=true
```


dotnet build -r osx.10.11-x64

自己完結型

```bash
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

Macaron.EDSのほうが良いかも。

Macaron.EDS

https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
