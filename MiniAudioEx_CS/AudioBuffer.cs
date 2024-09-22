using System.Runtime.CompilerServices;

namespace MiniAudioEx;

[method:
    MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly unsafe ref struct AudioBuffer<T>(void* pointer, int length)
    where T : unmanaged
{
    internal readonly void* _pointer = pointer;

    /// <summary>The number of elements this Span contains.</summary>
    public int Length { get; } = length;

    public bool IsEmpty => 0 >= (uint)Length;

    public ref T this[int index]
    {
        [MethodImpl(
            MethodImplOptions.AggressiveInlining)]
        get
        {
            if (index >= Length || index < 0)
                throw new System.IndexOutOfRangeException();
            return ref ((T*)_pointer)[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AudioBuffer(System.IntPtr pointer, int length) : this(pointer.ToPointer(), length)
    {
    }
}