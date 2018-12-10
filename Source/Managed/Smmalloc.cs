/*
 *  Managed C# wrapper for Smmalloc, blazing fast memory allocator designed for video games 
 *  Copyright (c) 2018 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Smmalloc {
	public enum CacheWarmupOptions {
		Cold = 0,
		Warm = 1,
		Hot = 2
	}

	public class SmmallocInstance : IDisposable {
		private IntPtr nativeAllocator;
		private readonly uint allocationLimit;

		public SmmallocInstance(uint bucketsCount, int bucketSize) {
			if (bucketsCount > 64)
				throw new ArgumentOutOfRangeException();

			nativeAllocator = Native.sm_allocator_create(bucketsCount, (IntPtr)bucketSize);

			if (nativeAllocator == IntPtr.Zero)
				throw new InvalidOperationException("Native memory allocator not created");

			allocationLimit = bucketsCount * 16;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (nativeAllocator != IntPtr.Zero) {
				Native.sm_allocator_destroy(nativeAllocator);
				nativeAllocator = IntPtr.Zero;
			}
		}

		~SmmallocInstance() {
			Dispose(false);
		}

		public void CreateThreadCache(int cacheSize, CacheWarmupOptions warmupOption) {
			if (cacheSize == 0 || cacheSize < 0)
				throw new ArgumentOutOfRangeException();

			Native.sm_allocator_thread_cache_create(nativeAllocator, warmupOption, (IntPtr)cacheSize);
		}

		public void DestroyThreadCache() {
			Native.sm_allocator_thread_cache_destroy(nativeAllocator);
		}

		#if SMMALLOC_INLINING
			[MethodImpl(256)]
		#endif
		public IntPtr Malloc(int bytesCount) {
			return Malloc(bytesCount, 0);
		}

		#if SMMALLOC_INLINING
			[MethodImpl(256)]
		#endif
		public IntPtr Malloc(int bytesCount, int alignment) {
			if (bytesCount == 0 || bytesCount < 0 || bytesCount > allocationLimit)
				throw new ArgumentOutOfRangeException();

			return Native.sm_malloc(nativeAllocator, (IntPtr)bytesCount, (IntPtr)alignment);
		}

		#if SMMALLOC_INLINING
			[MethodImpl(256)]
		#endif
		public void Free(IntPtr memory) {
			if (memory == IntPtr.Zero)
				throw new ArgumentNullException("memory");

			Native.sm_free(nativeAllocator, memory);
		}

		#if SMMALLOC_INLINING
			[MethodImpl(256)]
		#endif
		public IntPtr Realloc(IntPtr memory, int bytesCount) {
			return Realloc(memory, bytesCount, 0);
		}

		#if SMMALLOC_INLINING
			[MethodImpl(256)]
		#endif
		public IntPtr Realloc(IntPtr memory, int bytesCount, int alignment) {
			if (memory == IntPtr.Zero)
				throw new ArgumentNullException("memory");

			if (bytesCount == 0 || bytesCount < 0 || bytesCount > allocationLimit)
				throw new ArgumentOutOfRangeException();

			return Native.sm_realloc(nativeAllocator, memory, (IntPtr)bytesCount, (IntPtr)alignment);
		}

		#if SMMALLOC_INLINING
			[MethodImpl(256)]
		#endif
		public int Size(IntPtr memory) {
			if (memory == IntPtr.Zero)
				throw new ArgumentNullException("memory");

			return (int)Native.sm_msize(nativeAllocator, memory);
		}

		#if SMMALLOC_INLINING
			[MethodImpl(256)]
		#endif
		public int Bucket(IntPtr memory) {
			if (memory == IntPtr.Zero)
				throw new ArgumentNullException("memory");

			return Native.sm_mbucket(nativeAllocator, memory);
		}
	}

	[SuppressUnmanagedCodeSecurity]
	internal static class Native {
		private const string nativeLibrary = "smmalloc";

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sm_allocator_create(uint bucketsCount, IntPtr bucketSizeInBytes);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sm_allocator_destroy(IntPtr allocator);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sm_allocator_thread_cache_create(IntPtr allocator, CacheWarmupOptions warmupOption, IntPtr cacheSize);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sm_allocator_thread_cache_destroy(IntPtr allocator);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sm_malloc(IntPtr allocator, IntPtr bytesCount, IntPtr alignment);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sm_free(IntPtr allocator, IntPtr memory);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sm_realloc(IntPtr allocator, IntPtr memory, IntPtr bytesCount, IntPtr alignment);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sm_msize(IntPtr allocator, IntPtr memory);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int sm_mbucket(IntPtr allocator, IntPtr memory);
	}
}
