using System;
using System.Linq;
using System.Text;

class TestProgram
{
    private const string TEST_DATA = "The quick brown fox jumped over the lazy dog. The quick brown fox jumped over the lazy dog.";

    static void Main(string[] args)
    {
        byte[] data = Encoding.ASCII.GetBytes(TEST_DATA);

        byte[] compressedData = LZ4.CompressFrame(data);
        byte[] decompressedData = LZ4.DecompressFrame(compressedData);

        float ratio = 100.0f * (float)compressedData.Length / (float)data.Length;

        Console.WriteLine($"Compressed data info:\nCompression ratio: {ratio:.02}%");
        Console.WriteLine($"Original size: {data.Length}\nCompressed size: {compressedData.Length}");
        Console.WriteLine($"Original size: {data.Length}\nDecompressed size: {decompressedData.Length}");

        Console.WriteLine($"Decompression equivalence: {Enumerable.SequenceEqual(data, decompressedData)}");
    }
}
