using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace InsightDesk
{
    public static class InsightUtility
    {
        public static void Log(object message)
        {
            Debug.Log($"<color=#209756>[InsightDesk]</color> {message}");
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning($"<color=#209756>[InsightDesk]</color> {message}");
        }

        public static void LogError(object message)
        {
            Debug.LogError($"<color=#209756>[InsightDesk]</color> {message}");
        }

        public static string HashMesh(Mesh mesh)
        {
            if (!mesh.isReadable)
            {
                return "could_not_hash";
            }

            var rand = new System.Random(mesh.vertexCount);
            var hash = new Hash128();
            hash.Append(mesh.vertexCount);

            using (var dataArray = Mesh.AcquireReadOnlyMeshData(mesh))
            {
                var data = dataArray[0];

                using (var gotVertices = new NativeArray<Vector3>(mesh.vertexCount, Allocator.TempJob))
                {
                    data.GetVertices(gotVertices);
                    for (int j = 0; j < 20; j++)
                    {
                        hash.Append(gotVertices[rand.Next(0, gotVertices.Length)][rand.Next(3)]);
                    }
                }
            }

            return hash.ToString();
        }

        // https://stackoverflow.com/questions/39191950/how-to-compress-a-byte-array-without-stream-or-system-io
        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }

            return output.ToArray();
        }
    }
}