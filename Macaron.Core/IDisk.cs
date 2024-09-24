namespace Macaron.Core
{
    interface IDisk
    {
        void ConvertToImg();
        void ConvertFromImg();
        void ConvertFromVirtualDisk();
        void ConvertToVirtualDisk();
        void WriteDisk(string disknumber);
        void FormatPartition();
    }
}
