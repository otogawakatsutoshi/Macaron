# macaron-cli

macaron cli suply windows compatible move.
windowsimage, 

## how to use

```bash
macaron [eds | dism | disk] <options>
```

### eds

```bash
# downloadリストのeds。
macaron eds --download-eds --use-catalog catalog or url --model Macbook12.1,iMac12.3
macaron eds --download-catalog -u,-catalogurl url --outputdir
macaron eds -s, --show-currentmodel --use-catalog catalog or url


## 後で macのカタログを見て、　Macbook12.1　2016, Thunder boltなどのリスト
macaron eds --list --use-catalog

## イラン気がする。
macaron eds --install
```

### dism

macaron dism is syntqax for powershell DISM modules.
if you using powershell, you learn .

```bash
macaron dism --get-windowsimage --source --index
macaron dism --mount-windowsimage --source
macaron dism --dismount-windowsimage --source -d,--discared
macaron dism --expand-windowsimage --source --index
```

### hyperv

macaron 

```bash
macaron hyperv --mount-disk --source
macaron hyperv --dismount-disk
```

### qemu

macaron supply 

virtual disk to img file or rawdisk.

using

```bash
macaron qemu convert 
```


License GPL3
