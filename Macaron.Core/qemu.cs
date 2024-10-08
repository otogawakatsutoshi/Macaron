
namespace Macaron.Core
{
    // イメージ操作やディスク操作の関数からのみ操作する。QEMUの部分は最終的にはinternalにして公開しないようににする。
    internal class QEMU
    {
        internal static void ConvertToImg()
        {
            // require qemu-img
            // from vmdk, Virtualbox, hyper-v
            // to img
        }

        internal static void ConvertFromImg()
        {
            // qemu-img
            // to vmdk, virtualbox, hyper-v, wim
        }

        internal static void ConvertFromVirtualDisk()
        {
            // from vmdk, virtualbox, hyper-v
            // to img, wim
            // qemu-img
        }
        internal static void ConvertToVirtualDisk()
        {
            // qemu-img
            // from vmdk, virtualbox, hyper-v
            // to img, wim
        }

        internal static void WriteDisk(string disknumber)
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
        internal static void FormatPartition()
        {
            // DISK

        }
    }
}