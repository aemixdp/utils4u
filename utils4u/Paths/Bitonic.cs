using System;
using UnityEngine;

public static partial class Paths
{
    //
    // https://en.wikipedia.org/wiki/Bitonic_tour
    //
    public static Vector3[] BitonicTour(Vector3[] vertices)
    {
        if (vertices.Length < 4)
        {
            return vertices;
        }
        var vertexCount = vertices.Length;
        var sortedVertices = new Vector3[vertexCount];
        Array.Copy(vertices, sortedVertices, vertexCount);
        Array.Sort(sortedVertices, (a, b) => a.x.CompareTo(b.x));
        var bitonicDistances = new float[vertexCount * vertexCount];
        var bitonicPaths = new BitonicNode[vertexCount * vertexCount];
        bitonicDistances[0] = 0f;
        bitonicPaths[0] = BitonicNode.Nil;
        for (int i = 1; i < vertexCount; ++i)
        {
            bitonicDistances[i] = bitonicDistances[i - 1] +
                Vector3.Distance(sortedVertices[i - 1], sortedVertices[i]);
            bitonicPaths[i] = bitonicPaths[i - 1].Extend(i - 1, i, i - 1);
        }
        for (int i = 1; i < vertexCount; ++i)
        {
            for (int j = i; j < vertexCount; ++j)
            {
                int offset = i * vertexCount + j;
                if (i < j - 1)
                {
                    bitonicDistances[offset] = bitonicDistances[offset - 1] +
                        Vector3.Distance(sortedVertices[j - 1], sortedVertices[j]);
                    bitonicPaths[offset] = bitonicPaths[offset - 1].Extend(j - 1, j, offset - 1);
                }
                else
                {
                    var minDistance = float.MaxValue;
                    int bestVertexIndex = 0;
                    int bestVertexOffset = 0;
                    for (int k = 0; k < i; ++k)
                    {
                        int altOffset = k * vertexCount + i;
                        var tmp = bitonicDistances[altOffset] +
                            Vector3.Distance(sortedVertices[k], sortedVertices[j]);
                        if (tmp < minDistance)
                        {
                            minDistance = tmp;
                            bestVertexIndex = k;
                            bestVertexOffset = altOffset;
                        }
                    }
                    bitonicDistances[offset] = bitonicDistances[bestVertexOffset] +
                        Vector3.Distance(sortedVertices[bestVertexIndex], sortedVertices[j]);
                    bitonicPaths[offset] = bitonicPaths[bestVertexOffset]
                        .Extend(bestVertexIndex, j, bestVertexOffset);
                }
            }
        }
        return bitonicPaths[bitonicPaths.Length - 1]
            .BuildPath(bitonicPaths, sortedVertices);
    }

    private struct BitonicNode
    {
        enum DirectionType { Forward, Backward };

        int _fwPathTip;
        DirectionType _dir;
        int _curVertexIndex;
        int _prevNodeIndex;

        public static readonly BitonicNode Nil = new BitonicNode()
        {
            _fwPathTip = -1,
            _dir = DirectionType.Forward,
            _curVertexIndex = -1,
            _prevNodeIndex = -1
        };

        public BitonicNode Extend(int srcVertexIndex, int destVertexIndex, int prevCellIndex)
        {
            return (srcVertexIndex == _fwPathTip || _curVertexIndex == -1)
                ? Forward(srcVertexIndex, destVertexIndex, prevCellIndex)
                : Backward(destVertexIndex, destVertexIndex, prevCellIndex, _fwPathTip);
        }

        public T[] BuildPath<T>(BitonicNode[] backingArray, T[] domainVertices)
        {
            var len = domainVertices.Length;
            var results = new T[len];
            var node = this;
            int l = 0;
            int r = len - 1;
            for (;;)
            {
                switch (node._dir)
                {
                    case DirectionType.Forward:
                        results[l++] = domainVertices[node._curVertexIndex];
                        break;
                    case DirectionType.Backward:
                        results[r--] = domainVertices[node._curVertexIndex];
                        break;
                }
                node = backingArray[node._prevNodeIndex];
                if (node._curVertexIndex == -1)
                {
                    return results;
                }
            }
        }

        static BitonicNode Forward(int vertexIndex, int destVertexIndex, int prevCellIndex)
        {
            return new BitonicNode
            {
                _fwPathTip = destVertexIndex,
                _dir = DirectionType.Forward,
                _curVertexIndex = vertexIndex,
                _prevNodeIndex = prevCellIndex
            };
        }

        static BitonicNode Backward(int vertexIndex, int destVertexIndex, int prevCellIndex, int fwPathTip)
        {
            return new BitonicNode
            {
                _fwPathTip = fwPathTip,
                _dir = DirectionType.Backward,
                _curVertexIndex = vertexIndex,
                _prevNodeIndex = prevCellIndex
            };
        }
    }
}
