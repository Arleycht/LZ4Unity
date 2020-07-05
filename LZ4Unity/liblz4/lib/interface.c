#include "interface.h"

#include <stdio.h>
#include <string.h>

LZ4F_preferences_t Get_Preference(int compressionLevel)
{
	LZ4F_preferences_t pref = LZ4F_INIT_PREFERENCES;

	pref.compressionLevel = compressionLevel;

	return pref;
}

LZ4FLIB_API int unity_LZ4_versionNumber()
{
	return LZ4_versionNumber();
}

LZ4FLIB_API size_t unity_LZ4_versionString(char* buffer, size_t bufferSize)
{
	const char* version = (const char*)LZ4_versionString();
	size_t len = strlen(version);

	strncpy_s(buffer, bufferSize, version, len);

	return len;
}

LZ4FLIB_API size_t unity_LZ4F_compress(const char* src, size_t srcLen, char* dst, size_t dstLen, int compressionLevel)
{
	LZ4F_preferences_t pref = Get_Preference(compressionLevel);

	return LZ4F_compressFrame(dst, dstLen, src, srcLen, &pref);
}

LZ4FLIB_API size_t unity_LZ4F_decompress(char** dst, const char* src, size_t srcSize)
{
	size_t bufferSize = 256;

	int errCode = 0;

	LZ4F_dctx* dctx;

	LZ4F_createDecompressionContext(&dctx, LZ4F_VERSION);

	size_t totalBytesConsumed = 0;
	size_t totalBytesRecorded = 0;

	size_t bytesConsumed = 0;
	size_t bytesRegenerated = bufferSize;

	char* buffer = malloc(bytesRegenerated * sizeof(char));
	char* result = malloc(sizeof(char));

	while (1)
	{
		size_t hint = LZ4F_decompress(dctx, buffer, &bytesRegenerated, src + totalBytesConsumed, &bytesConsumed, 0);

		totalBytesConsumed += bytesConsumed;

		if (LZ4F_isError(hint))
		{
			errCode = hint;

			break;
		}

		if (bytesRegenerated > 0)
		{
			char* newResultPtr = realloc(result, (totalBytesRecorded + bytesRegenerated) * sizeof(char));

			if (newResultPtr == NULL)
			{
				errCode = -9;

				break;
			}

			result = newResultPtr;

			memcpy(result + totalBytesRecorded, buffer, bytesRegenerated);

			totalBytesRecorded += bytesRegenerated;
		}

		if (hint > 0)
		{
			// Setup for next iteration

			bytesConsumed = hint;
			bytesRegenerated = bufferSize;

			if (totalBytesConsumed + hint > srcSize)
			{
				bytesConsumed = srcSize - totalBytesConsumed;
			}
		}
		else if (hint == 0)
		{
			break;
		}
	}

	if (errCode < 0)
	{
		free(buffer);
		free(result);

		printf("Error encountered, returning...\n");

		return errCode;
	}

	(*dst) = result;

	free(buffer);

	LZ4F_freeDecompressionContext(dctx);

	return totalBytesRecorded;
}

LZ4FLIB_API void unity_LZ4F_freeDecompressBuffer(char* buffer)
{
	free(buffer);
}

LZ4FLIB_API size_t unity_LZ4F_compressBound(int srcSize, int compressionLevel)
{
	LZ4F_preferences_t pref = Get_Preference(compressionLevel);

	return LZ4F_compressBound(srcSize, &pref);
}

LZ4FLIB_API unsigned int unity_LZ4F_isError(size_t code)
{
	return LZ4F_isError(code);
}

LZ4FLIB_API size_t unity_LZ4F_getErrorName(int code, char* buffer, size_t bufferSize)
{
	const char* errName = LZ4F_getErrorName(code);
	size_t len = strlen(errName);

	strncpy_s(buffer, bufferSize, errName, len);

	return len;
}
