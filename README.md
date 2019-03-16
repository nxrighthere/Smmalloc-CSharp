<p align="center"> 
  <img src="https://i.imgur.com/7XvtEWf.png" alt="alt logo">
</p>

[![PayPal](https://drive.google.com/uc?id=1OQrtNBVJehNVxgPf6T6yX1wIysz1ElLR)](https://www.paypal.me/nxrighthere) [![Bountysource](https://drive.google.com/uc?id=19QRobscL8Ir2RL489IbVjcw3fULfWS_Q)](https://salt.bountysource.com/checkout/amount?team=nxrighthere) [![Coinbase](https://drive.google.com/uc?id=1LckuF-IAod6xmO9yF-jhTjq1m-4f7cgF)](https://commerce.coinbase.com/checkout/03e11816-b6fc-4e14-b974-29a1d0886697) [![Discord](https://discordapp.com/api/guilds/515987760281288707/embed.png)](https://discord.gg/ceaWXVw)

This is an improved version of [smmalloc](https://github.com/SergeyMakeev/smmalloc) a [fast and efficient](https://github.com/SergeyMakeev/smmalloc#features) memory allocator designed to handle many small allocations/deallocations in heavy multi-threaded scenarios. The allocator created for usage in applications where the performance is critical such as video games.

Using smmalloc allocator in the .NET environment helps to minimize GC pressure for allocating buffers and avoid using lock-based pools in multi-threaded systems. Modern .NET features such as [`Span<T>`](https://msdn.microsoft.com/en-us/magazine/mt814808.aspx) greatly works in tandem with smmalloc and allows conveniently manage data in native memory blocks.

Usage
--------
##### Create a new smmalloc instance:
```c#
// 8 buckets, 16 MB each, 128 bytes maximum allocation size
SmmallocInstance smmalloc = new SmmallocInstance(8, 16 * 1024 * 1024);
```

##### Destroy the smmalloc instance and free allocated memory:
```c#
smmalloc.Dispose();
```

##### Create thread cache for a current thread:
```c#
// 4 KB of thread cache for each bucket, hot warmup
smmalloc.CreateThreadCache(4 * 1024, CacheWarmupOptions.Hot);
```

##### Destroy thread cache for a current thread:
```c#
smmalloc.DestroyThreadCache();
```

##### Allocate memory block:
```c#
// 64 bytes of a memory block
IntPtr memory = smmalloc.Malloc(64);
```

##### Release memory block:
```c#
smmalloc.Free(memory);
```

##### Work with batches of memory blocks:
```c#
IntPtr[] batch = new IntPtr[32];

// Allocate a batch of memory
for (int i = 0; i < batch.Length; i++) {
	batch[i] = smmalloc.Malloc(64);
}

// Release the whole batch
smmalloc.Free(batch);
```

##### Write data to memory block:
```c#
// Using Marshal
byte data = 0;

for (int i = 0; i < smmalloc.Size(memory); i++) {
	Marshal.WriteByte(memory, i, data++);
}

// Using Span
Span<byte> buffer;

unsafe {
	buffer = new Span<byte>((byte*)memory, smmalloc.Size(memory));
}

byte data = 0;

for (int i = 0; i < buffer.Length; i++) {
	buffer[i] = data++;
}
```

##### Read data from memory block:
```c#
// Using Marshal
int sum = 0;

for (int i = 0; i < smmalloc.Size(memory); i++) {
	sum += Marshal.ReadByte(memory, i);
}

// Using Span
int sum = 0;

foreach (var value in buffer) {
	sum += value;
}
```

##### Hardware accelerated operations:
```c#
// Xor using Vector and Span
if (Vector.IsHardwareAccelerated) {
	Span<Vector<byte>> bufferVector = MemoryMarshal.Cast<byte, Vector<byte>>(buffer);
	Span<Vector<byte>> xorVector = MemoryMarshal.Cast<byte, Vector<byte>>(xor);

	for (int i = 0; i < bufferVector.Length; i++) {
		bufferVector[i] ^= xorVector[i];
	}
}
```

##### Copy data using memory block:
```c#
// Using Marshal
byte[] data = new byte[64];

// Copy from native memory
Marshal.Copy(memory, data, 0, 64);

// Copy to native memory
Marshal.Copy(data, 0, memory, 64);

// Using Buffer
unsafe {
	// Copy from native memory
	fixed (byte* destination = &data[0]) {
		Buffer.MemoryCopy((byte*)memory, destination, 64, 64);
	}

	// Copy to native memory
	fixed (byte* source = &data[0]) {
		Buffer.MemoryCopy(source, (byte*)memory, 64, 64);
	}
}
```

##### Custom data structures:
```c#
// Define a custom structure
struct Entity {
	public uint id;
	public byte health;
	public byte state;
}

int entitySize = Marshal.SizeOf(typeof(Entity));
int entityCount = 10;

// Allocate memory block
IntPtr memory = smmalloc.Malloc(entitySize * entityCount);

// Create Span using native memory block
Span<Entity> entities;

unsafe {
	entities = new Span<Entity>((byte*)memory, entityCount);
}

// Do some stuff
uint id = 1;

for (int i = 0; i < entities.Length; i++) {
	entities[i].id = id++;
	entities[i].health = (byte)(new Random().Next(1, 100));
	entities[i].state = (byte)(new Random().Next(1, 255));
}

// Release memory block
smmalloc.Free(memory);
```

API reference
--------
### Enumerations
#### CacheWarmupOptions
Definitions of warmup options for `CreateThreadCache()` function:

`CacheWarmupOptions.Cold` warmup not performed for cache elements.

`CacheWarmupOptions.Warm` warmup performed for half of the cache elements.

`CacheWarmupOptions.Hot` warmup performed for all cache elements.

### Classes
A single low-level disposable class is used to work with smmalloc. 

#### SmmallocInstance
Contains a managed pointer to the smmalloc instance.

##### Constructors
`SmmallocInstance(uint bucketsCount, int bucketSize)` creates allocator instance with a memory pool. Size of memory blocks in each bucket increases with a count of buckets. The bucket size parameter sets an initial size of a pooled memory in bytes.

##### Methods
`SmmallocInstance.Dispose()` destroys the smmalloc instance and frees allocated memory.

`SmmallocInstance.CreateThreadCache(int cacheSize, CacheWarmupOptions warmupOption)` creates thread cache for fast memory allocations within a thread. The warmup option sets pre-allocation degree of cache elements.

`SmmallocInstance.DestroyThreadCache()` destroys the thread cache. Should be called before the end of the thread's life cycle.

`SmmallocInstance.Malloc(int bytesCount, int alignment)` allocates aligned memory block. Allocation size depends on buckets count multiplied by 16, so the minimum allocation size is 16 bytes. Maximum allocation size using two buckets in a smmalloc instance will be 32 bytes, for three buckets 48 bytes, for four 64 bytes, and so on. The alignment parameter is optional. Returns pointer to a memory block. Returns a pointer to an allocated memory block.

`SmmallocInstance.Free(IntPtr memory)` frees memory block. A managed array or pointer to pointers with length can be used instead of a pointer to memory block to free a batch of memory.

`SmmallocInstance.Realloc(IntPtr memory, int bytesCount, int alignment)` reallocates memory block. The alignment parameter is optional. Returns a pointer to a reallocated memory block.

`SmmallocInstance.Size(IntPtr memory)` gets usable memory size. Returns size in bytes.

`SmmallocInstance.Bucket(IntPtr memory)` gets bucket index of a memory block. Returns placement index.
