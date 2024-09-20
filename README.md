


tool build

```bash
dotnet pack -c Release /p:PackAsTool=true
```

自己完結型

```bash
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

Macaron.EDSのほうが良いかも。

Macaron.EDS
