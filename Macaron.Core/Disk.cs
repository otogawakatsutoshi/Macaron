namespace Macaron.Core
{
    public class Disk
    {
        public void ConvertToImg()
        {
            
            // from vmdk, Virtualbox, hyper-v
            // to img
            QEMU.ConvertToImg();
            // require qemu-img
        }

        public void ConvertFromImg()
        {
            // qemu-img
            // to vmdk, virtualbox, hyper-v, wim
        }

        public void ConvertFromVirtualDisk()
        {
            // from vmdk, virtualbox, hyper-v
            // to img, wim
            // qemu-img
        }
        public void ConvertToVirtualDisk()
        {
            // qemu-img
            // from vmdk, virtualbox, hyper-v
            // to img, wim
        }

        public  void WriteDisk(string disknumber)
        {
            // in vmdk, virtualbox, hyper-v, img
            // out disk

            // raw "$disk"
// # .imgファイルを指定
// img_file="/path/to/image.img"

// # 書き込み先のディスクを指定（物理ディスク。ディスク番号は適宜確認する）
// disk="/dev/sdX"  # 例: /dev/sdb

// # imgファイルを物理ディスクに直接書き込み
// sudo qemu-img convert -f raw "$img_file" -O raw "$disk"
        }
        public void FormatPartition()
        {
            // DISK

        }
    }
}
