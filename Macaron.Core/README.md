# Macaron.Core

## how to build


dism module dependent https://wimlib.net/

```bash
git submodule 
```


  <!-- 各プラットフォームごとにネイティブライブラリを条件付きでバンドル -->
  <ItemGroup Condition="'$(RuntimeIdentifier)' == 'win-x64'">
    <None Update="native/windows/MyNativeLibrary.dll">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <ItemGroup Condition="'$(RuntimeIdentifier)' == 'linux-x64'">
    <None Update="native/linux/libMyNativeLibrary.so">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <ItemGroup Condition="'$(RuntimeIdentifier)' == 'osx-x64'">
    <None Update="native/osx/libMyNativeLibrary.dylib">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
