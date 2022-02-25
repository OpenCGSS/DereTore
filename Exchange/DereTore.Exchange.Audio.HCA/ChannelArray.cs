using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DereTore.Common;

namespace DereTore.Exchange.Audio.HCA {
    internal sealed unsafe class ChannelArray : DisposableBase {

        static ChannelArray() {
        }

        public ChannelArray(int channelCount) {
            ChannelCount = channelCount;

            var totalSize = this.totalSize = channelCount * ChannelSize;

            _basePtr = Marshal.AllocHGlobal(totalSize);

            ZeroMemory(_basePtr.ToPointer(), totalSize);
        }

        // ref: https://stackoverflow.com/questions/15975972/copy-data-from-from-intptr-to-intptr
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        private ChannelArray(IntPtr ptr, int totalSize)
        {
            if (totalSize <= 0) throw new ArgumentOutOfRangeException(nameof(totalSize));
            // Clone internal states
            _basePtr = Marshal.AllocHGlobal(totalSize);
            CopyMemory(_basePtr, ptr, (uint)totalSize);
            this.totalSize = totalSize;
        }

        public int ChannelCount { get; }

        public void Decode1(int channelIndex, DataBits data, uint a, int b, byte[] ath) {
            var v = data.GetBit(3);
            var pCount = GetPtrOfCount(channelIndex);
            var pValue = GetPtrOfValue(channelIndex);

            if (v >= 6) {
                for (var i = 0; i < *pCount; ++i) {
                    pValue[i] = (sbyte)data.GetBit(6);
                }
            } else if (v != 0) {
                var v1 = data.GetBit(6);
                var v2 = (1 << v) - 1;
                var v3 = v2 >> 1;

                pValue[0] = (sbyte)v1;

                for (var i = 1; i < *pCount; ++i) {
                    var v4 = data.GetBit(v);

                    if (v4 != v2) {
                        v1 += v4 - v3;
                    } else {
                        v1 = data.GetBit(6);
                    }

                    pValue[i] = (sbyte)v1;
                }
            } else {
                ZeroMemory(pValue, 0x80);
            }

            var pType = GetPtrOfType(channelIndex);
            var pValue2 = GetPtrOfValue2(channelIndex);
            var ppValue3 = GetPtrOfValue3(channelIndex);

            if (*pType == 2) {
                v = data.CheckBit(4);
                pValue2[0] = (sbyte)v;

                if (v < 15) {
                    for (var i = 0; i < 8; ++i) {
                        pValue2[i] = (sbyte)data.GetBit(4);
                    }
                }
            } else {
                for (var i = 0; i < a; ++i) {
                    (*ppValue3)[i] = (sbyte)data.GetBit(6);
                }
            }

            var pScale = GetPtrOfScale(channelIndex);

            for (var i = 0; i < *pCount; ++i) {
                v = pValue[i];

                if (v != 0) {
                    v = ath[i] + ((b + i) >> 8) - ((v * 5) >> 1) + 1;

                    if (v < 0) {
                        v = 15;
                    } else if (v >= 0x39) {
                        v = 1;
                    } else {
                        v = ChannelTables.Decode1ScaleList[v];
                    }
                }

                pScale[i] = (sbyte)v;
            }

            ZeroMemory(&pScale[*pCount], (int)(0x80 - *pCount));

            var pBase = GetPtrOfBase(channelIndex);

            for (var i = 0; i < *pCount; ++i) {
                pBase[i] = ChannelTables.Decode1ValueSingle[pValue[i]] * ChannelTables.Decode1ScaleSingle[pScale[i]];
            }
        }

        public void Decode2(int channelIndex, DataBits data) {
            var pCount = GetPtrOfCount(channelIndex);
            var pScale = GetPtrOfScale(channelIndex);
            var pBlock = GetPtrOfBlock(channelIndex);
            var pBase = GetPtrOfBase(channelIndex);

            for (var i = 0; i < *pCount; ++i) {
                int s = pScale[i];
                int bitSize = ChannelTables.Decode2List1[s];
                var v = data.GetBit(bitSize);
                float f;

                if (s < 8) {
                    v += s << 4;
                    data.AddBit(ChannelTables.Decode2List2[v] - bitSize);
                    f = ChannelTables.Decode2List3[v];
                } else {
                    v = (1 - ((v & 1) << 1)) * (v >> 1);

                    if (v == 0) {
                        data.AddBit(-1);
                    }

                    f = v;
                }

                pBlock[i] = pBase[i] * f;
            }

            ZeroMemory(&pBlock[*pCount], sizeof(float) * (int)(0x80 - *pCount));
        }

        public void Decode3(int channelIndex, uint a, uint b, uint c, uint d) {
            var pType = GetPtrOfType(channelIndex);

            if (*pType != 2 && b != 0) {
                fixed (float* listFloatBase = ChannelTables.Decode3ListSingle) {
                    var pBlock = GetPtrOfBlock(channelIndex);
                    var ppValue3 = GetPtrOfValue3(channelIndex);
                    var pValue = GetPtrOfValue(channelIndex);
                    var listFloat = listFloatBase + 0x40;

                    var k = c;
                    var l = c - 1;

                    for (var i = 0; i < a; ++i) {
                        for (var j = 0; j < b && k < d; ++j, --l) {
                            pBlock[k++] = listFloat[(*ppValue3)[i] - pValue[l]] * pBlock[l];
                        }
                    }

                    pBlock[0x80 - 1] = 0;
                }
            }
        }

        public void Decode4(int channelIndex1, int channelIndex2, int index, uint a, uint b, uint c) {
            var pTypeA = GetPtrOfType(channelIndex1);

            if (*pTypeA == 1 && c != 0) {
                var pValue2B = GetPtrOfValue2(channelIndex2);
                var pBlockA = GetPtrOfBlock(channelIndex1);
                var pBlockB = GetPtrOfBlock(channelIndex2);

                var f1 = ChannelTables.Decode4ListSingle[pValue2B[index]];
                var f2 = f1 - 2.0f;

                var s = &pBlockA[b];
                var d = &pBlockB[b];

                for (var i = 0; i < a; ++i) {
                    *(d++) = *s * f2;
                    *(s++) = *s * f1;
                }
            }
        }

        public void Decode5(int channelIndex, int index) {
            float* s, d;

            s = GetPtrOfBlock(channelIndex);
            d = GetPtrOfWav1(channelIndex);

            var count1 = 1;
            var count2 = 0x40;

            for (var i = 0; i < 7; ++i, count1 <<= 1, count2 >>= 1) {
                var d1 = d;
                var d2 = &d[count2];

                for (var j = 0; j < count1; ++j) {
                    for (var k = 0; k < count2; ++k) {
                        var a = *(s++);
                        var b = *(s++);

                        *(d1++) = b + a;
                        *(d2++) = a - b;
                    }

                    d1 += count2;
                    d2 += count2;
                }

                var w = &s[-0x80];
                s = d;
                d = w;
            }

            s = GetPtrOfWav1(channelIndex);
            d = GetPtrOfBlock(channelIndex);

            fixed (float* list1FloatBase = ChannelTables.Decode5List1Single) {
                fixed (float* list2FloatBase = ChannelTables.Decode5List2Single) {
                    count1 = 0x40;
                    count2 = 1;

                    for (var i = 0; i < 7; ++i, count1 >>= 1, count2 <<= 1) {
                        var list1Float = &list1FloatBase[i * 0x40];
                        var list2Float = &list2FloatBase[i * 0x40];

                        var s1 = s;
                        var s2 = &s1[count2];
                        var d1 = d;
                        var d2 = &d1[count2 * 2 - 1];

                        for (var j = 0; j < count1; ++j) {
                            for (var k = 0; k < count2; ++k) {
                                var fa = *(s1++);
                                var fb = *(s2++);
                                var fc = *(list1Float++);
                                var fd = *(list2Float++);

                                *(d1++) = fa * fc - fb * fd;
                                *(d2--) = fa * fd + fb * fc;
                            }

                            s1 += count2;
                            s2 += count2;
                            d1 += count2;
                            d2 += count2 * 3;
                        }

                        var w = s;
                        s = d;
                        d = w;
                    }
                }
            }

            d = GetPtrOfWav2(channelIndex);

            for (var i = 0; i < 0x80; ++i) {
                *(d++) = *(s++);
            }

            fixed (float* list3FloatBase = ChannelTables.Decode5List3Single) {
                s = list3FloatBase;
                d = GetPtrOfWave(channelIndex) + index * 0x80;

                var s1 = &GetPtrOfWav2(channelIndex)[0x40];
                var s2 = GetPtrOfWav3(channelIndex);

                for (var i = 0; i < 0x40; ++i) {
                    *(d++) = *(s1++) * *(s++) + *(s2++);
                }

                for (var i = 0; i < 0x40; ++i) {
                    *(d++) = *(s++) * *(--s1) - *(s2++);
                }

                s1 = &GetPtrOfWav2(channelIndex)[0x40 - 1];
                s2 = GetPtrOfWav3(channelIndex);

                for (var i = 0; i < 0x40; ++i) {
                    *(s2++) = *(s1--) * *(--s);
                }

                for (var i = 0; i < 0x40; ++i) {
                    *(s2++) = *(--s) * *(++s1);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* GetBasePtr(int channelIndex) {
            return (void*)(_basePtr + ChannelSize * channelIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetPtrOfBlock(int channelIndex) {
            return (float*)GetPtrOf(channelIndex, OffsetOfBlock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetPtrOfBase(int channelIndex) {
            return (float*)GetPtrOf(channelIndex, OffsetOfBase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte* GetPtrOfValue(int channelIndex) {
            return (sbyte*)GetPtrOf(channelIndex, OffsetOfValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte* GetPtrOfScale(int channelIndex) {
            return (sbyte*)GetPtrOf(channelIndex, OffsetOfScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte* GetPtrOfValue2(int channelIndex) {
            return (sbyte*)GetPtrOf(channelIndex, OffsetOfValue2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int* GetPtrOfType(int channelIndex) {
            return (int*)GetPtrOf(channelIndex, OffsetOfType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte** GetPtrOfValue3(int channelIndex) {
            return (sbyte**)GetPtrOf(channelIndex, OffsetOfValue3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint* GetPtrOfCount(int channelIndex) {
            return (uint*)GetPtrOf(channelIndex, OffsetOfCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetPtrOfWav1(int channelIndex) {
            return (float*)GetPtrOf(channelIndex, OffsetOfWav1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetPtrOfWav2(int channelIndex) {
            return (float*)GetPtrOf(channelIndex, OffsetOfWav2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetPtrOfWav3(int channelIndex) {
            return (float*)GetPtrOf(channelIndex, OffsetOfWav3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetPtrOfWave(int channelIndex) {
            return (float*)GetPtrOf(channelIndex, OffsetOfWave);
        }

        protected override void Dispose(bool disposing) {
            if (_basePtr != IntPtr.Zero) {
                Marshal.FreeHGlobal(_basePtr);
            }

            _basePtr = IntPtr.Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntPtr GetPtrOf(int channelIndex, IntPtr fieldOffset) {
            return _basePtr + ChannelSize * channelIndex + fieldOffset.ToInt32();
        }

        private static void ZeroMemory(void* ptr, int byteCount) {
            if (ptr == null || byteCount <= 0) {
                return;
            }

            var p = (byte*)ptr;

            for (var i = 0; i < byteCount; ++i) {
                p[i] = 0;
            }
        }

        public ChannelArray clone()
        {
            return new ChannelArray(_basePtr, totalSize);
        }

        private static readonly int ChannelSize = Marshal.SizeOf(typeof(Channel));

        private static readonly IntPtr OffsetOfBlock = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Block));

        private static readonly IntPtr OffsetOfBase = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Base));

        private static readonly IntPtr OffsetOfValue = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Value));

        private static readonly IntPtr OffsetOfScale = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Scale));

        private static readonly IntPtr OffsetOfValue2 = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Value2));

        private static readonly IntPtr OffsetOfType = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Type));

        private static readonly IntPtr OffsetOfValue3 = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Value3));

        private static readonly IntPtr OffsetOfCount = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Count));

        private static readonly IntPtr OffsetOfWav1 = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Wav1));

        private static readonly IntPtr OffsetOfWav2 = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Wav2));

        private static readonly IntPtr OffsetOfWav3 = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Wav3));

        private static readonly IntPtr OffsetOfWave = Marshal.OffsetOf(typeof(Channel), nameof(Channel.Wave));

        private IntPtr _basePtr;
        private int totalSize;

    }
}
