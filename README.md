This project is a pure C# translation of [enet](https://github.com/lsalzman/enet), no binaries.

Where udp implementation: [NativeSockets](https://www.nuget.org/packages/NativeSockets).

**It is fully wire‑compatible with the original C library.**

## Why?

The original ENet relies on platform-specific native binaries,

which poses challenges for cross-platform distribution and deployment.

This project eliminates that dependency while preserving full compatibility with the original implementation.

---

## xENet

- The pure C# port of **ENet**. 
- It keeps the original low-level, C-style API and is fully wire-compatible with the original C library.

[![NuGet](https://img.shields.io/nuget/v/xENet.svg?style=flat-square)](https://www.nuget.org/packages/xENet/)

---

## yENet

- A managed wrapper around **xENet**. 
- It provides a modern C# API instead of the original C-style API.

[![NuGet](https://img.shields.io/nuget/v/yENet.svg?style=flat-square)](https://www.nuget.org/packages/yENet/)

---

## zENet

- A multithreaded wrapper around **yENet**. 
- It adds multithreading support on top of **yENet**.

[![NuGet](https://img.shields.io/nuget/v/zENet.svg?style=flat-square)](https://www.nuget.org/packages/zENet/)