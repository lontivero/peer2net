namespace P2PNet.BufferManager
{
    internal interface IBufferAllocator
    {
        Buffer Allocate(int size);
        void Free(Buffer segments);
    }
}