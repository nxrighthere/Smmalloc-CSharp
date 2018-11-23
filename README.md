<p align="center"> 
  <img src="https://i.imgur.com/7XvtEWf.png" alt="alt logo">
</p>

[![PayPal](https://drive.google.com/uc?id=1OQrtNBVJehNVxgPf6T6yX1wIysz1ElLR)](https://www.paypal.me/nxrighthere) [![Bountysource](https://drive.google.com/uc?id=19QRobscL8Ir2RL489IbVjcw3fULfWS_Q)](https://salt.bountysource.com/checkout/amount?team=nxrighthere)

This is an improved version of [smmalloc](https://github.com/SergeyMakeev/smmalloc) a [fast and efficient](https://github.com/SergeyMakeev/smmalloc#features) memory allocator designed to handle many small allocations/deallocations in heavy multi-threaded scenarios. The allocator created for using in applications where the performance is critical such as video games.

Using smmalloc allocator in the .NET environment helps to minimize GC pressure for allocating buffers and avoid using lock-based pools in multi-threaded systems. Modern .NET features such as Span<T> greatly works in tandem with smmalloc and allows conveniently manage data in native memory.
  

Usage
--------
