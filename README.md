


tool build

```bash
dotnet pack -c Release /p:PackAsTool=true
```


dotnet build -r osx.10.11-x64

自己完結型

```bash
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true

dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```


https://learn.microsoft.com/en-us/dotnet/core/rid-catalog

## tool debug build

tool build

```bash
dotnet pack -c Debug /p:PackAsTool=true
```

install 

```bash
dotnet tool install --global --add-source $(pwd)/bin/Debug/ macaron --version 1.0.0-pre 
```



cat << \EOF >> ~/.zprofile           
# Add .NET Core SDK tools
export PATH="$PATH:/Users/katsutoshi/.dotnet/tools"
EOF

dotnet tool uninstall -g macaron
