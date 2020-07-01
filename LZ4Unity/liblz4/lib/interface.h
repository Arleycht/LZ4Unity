#ifndef INTERFACE_H
#define INTERFACE_H

#include "lz4frame.h"

#define LZ4UNITY_API __declspec(dllexport)

LZ4UNITY_API int unity_LZ4_versionNumber();
LZ4UNITY_API size_t unity_LZ4_versionString(char* buffer, size_t bufferSize);

LZ4UNITY_API size_t unity_LZ4F_compress(const char* src, size_t srcLen, char* dst, size_t dstLen, int compressionLevel);
LZ4UNITY_API size_t unity_LZ4F_decompress(char** dst, const char* src, size_t srcSize);
LZ4UNITY_API void unity_LZ4F_freeDecompressBuffer(char* buffer);

LZ4UNITY_API size_t unity_LZ4F_compressBound(int srcSize, int compressionLevel);

LZ4UNITY_API unsigned int unity_LZ4F_isError(size_t code);
LZ4UNITY_API size_t unity_LZ4F_getErrorName(int code, char* buffer, size_t bufferSize);

#endif