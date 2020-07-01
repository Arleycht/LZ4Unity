using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// <br>A wrapper for the LZ4 library.</br>
/// </summary>
public static class LZ4
{
    private static class API
    {
        private const string DLL_NAME = "liblz4";

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unity_LZ4_versionNumber();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unity_LZ4_versionString(byte[] buffer, int bufferSize);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unity_LZ4F_compress(byte[] src, int srcSize, byte[] dst, int dstSize, int compressionLevel);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unity_LZ4F_decompress(out IntPtr dst, byte[] src, int srcSize);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void unity_LZ4F_freeDecompressBuffer(IntPtr buffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unity_LZ4F_compressBound(byte[] src, int srcSize, int compressionLevel);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool unity_LZ4F_isError(int code);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int unity_LZ4F_getErrorName(int code, byte[] buffer, int bufferSize);
    }

    /// <summary>
    /// The last error message that was generated.
    /// </summary>
    public static string LastErrorMessage { get; private set; }

    private static void LogError(int code)
    {
        if (IsError(code))
        {
            LastErrorMessage = GetErrorName(code);
        }
        else
        {
            LastErrorMessage = $"Unknown error code {code}";
        }
    }

    private static void LogError(string message)
    {
        LastErrorMessage = message;
    }

    private static bool IsError(int code)
    {
        return API.unity_LZ4F_isError(code);
    }

    private static string GetErrorName(int code)
    {
        byte[] buffer = new byte[128];
        int len = API.unity_LZ4F_getErrorName(code, buffer, buffer.Length);

        return Encoding.ASCII.GetString(buffer).Substring(0, (int)len);
    }

    /// <summary>
    /// Get the version of LZ4 that LZ4Unity is using as a number.
    /// </summary>
    /// <returns></returns>
    public static int GetVersionNumber()
    {
        return API.unity_LZ4_versionNumber();
    }

    /// <summary>
    /// Get the version of LZ4 that LZ4Unity is using.
    /// </summary>
    /// <returns></returns>
    public static string GetVersionString()
    {
        byte[] buffer = new byte[16];

        int len = API.unity_LZ4_versionString(buffer, buffer.Length);

        return Encoding.ASCII.GetString(buffer).Substring(0, len);
    }

    /// <summary>
    /// Compresses source data using the LZ4 frame format.
    /// </summary>
    /// <param name="src">
    /// A byte array.
    /// </param>
    /// <param name="compressionLevel">
    /// <br>Common compression levels are:</br>
    /// <br>-1 : fast compression</br>
    /// <br>-9 : high compression</br>
    /// </param>
    /// <returns>
    /// A compressed byte array.
    /// </returns>
    public static byte[] CompressFrame(byte[] src, int compressionLevel = -1)
    {
        byte[] dst = null;

        if (src != null && src.Length > 0)
        {
            int bufferSize = API.unity_LZ4F_compressBound(src, src.Length, compressionLevel);

            if (bufferSize > 0)
            {
                byte[] buffer = new byte[bufferSize];

                int actualSize = API.unity_LZ4F_compress(src, src.Length, buffer, buffer.Length, compressionLevel);

                if (actualSize > 0)
                {
                    dst = new byte[actualSize];

                    Array.Copy(buffer, dst, dst.Length);
                }
                else
                {
                    LogError(actualSize);
                }
            }
            else
            {
                LogError(bufferSize);
            }
        }

        return dst;
    }

    /// <summary>
    /// Decompresses compressed data using the LZ4 frame format.<br></br>
    /// Supports any compression level.<br></br>
    /// </summary>
    /// <param name="src"></param>
    /// <returns>Byte representing the decompressed data.</returns>
    public static byte[] DecompressFrame(byte[] src)
    {
        byte[] dst = null;

        if (src != null && src.Length > 0)
        {
            int bufferSize = API.unity_LZ4F_decompress(
                out IntPtr bufferPtr,
                src,
                src.Length
            );

            if (IsError(bufferSize))
            {
                LogError(bufferSize);

                API.unity_LZ4F_freeDecompressBuffer(bufferPtr);

                return dst;
            }

            dst = new byte[bufferSize];

            try
            {
                Marshal.Copy(bufferPtr, dst, 0, bufferSize);
            }
            catch (Exception e)
            {
                LogError($"Exception while copying decompression buffer: {e.Message}");

                dst = null;
            }

            API.unity_LZ4F_freeDecompressBuffer(bufferPtr);
        }

        return dst;
    }
}
