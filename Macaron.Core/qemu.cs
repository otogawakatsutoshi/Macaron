
namespace Macaron.Core
{
    public class QEMU
    {
        public static void ConvertToImg()
        {
            // require qemu-img
            // from vmdk, Virtualbox, hyper-v
            // to img
        }

        public static void ConvertFromImg()
        {
            // qemu-img
            // to vmdk, virtualbox, hyper-v, wim
        }

        public static void ConvertFromVirtualDisk()
        {
            // from vmdk, virtualbox, hyper-v
            // to img, wim
            // qemu-img
        }
        public static void ConvertToVirtualDisk()
        {
            // qemu-img
            // from vmdk, virtualbox, hyper-v
            // to img, wim
        }

        public static void WriteDisk(string disknumber)
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
        public static void FormatPartition()
        {
            // DISK

        }
    }
}