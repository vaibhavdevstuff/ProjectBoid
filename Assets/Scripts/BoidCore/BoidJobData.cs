using System;
using System.Collections.Generic;
using ProjectBoid.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace ProjectBoid.BoidCore
{
    public class BoidJobData
    {
        private NativeArray<BoidJobUnit> _boidJobUnits;
        private NativeArray<HashAndIndex> _hashAndIndices;
        private NativeArray<UnsafeList<int>> _neighbourIndices;
        
        public NativeArray<UnsafeList<int>> NeighbourIndices => _neighbourIndices;

        public struct BoidJobUnit
        {
            public float3 Position;
        }

        public struct HashAndIndex : IComparable<HashAndIndex>
        {
            public int Hash;
            public int Index;

            public int CompareTo(HashAndIndex other)
            {
                return Hash.CompareTo(other.Hash);
            }
        }

        public void SetUnitPosition(int index, float3 position)
        {
            var unit = _boidJobUnits[index];
            unit.Position = position;
            _boidJobUnits[index] = unit;
        }

        public void GetNeighbourIndices(int index, List<int> result)
        {
            result.Clear();
            var unsafeList = _neighbourIndices[index];
            
            for (var i = 0; i < unsafeList.Length; i++)
                result.Add(unsafeList[i]);
        }

        
        public void Init(BoidDataSO boidData)
        {
            if (_boidJobUnits.IsCreated) _boidJobUnits.Dispose();
            if (_hashAndIndices.IsCreated) _hashAndIndices.Dispose();
            if (_neighbourIndices.IsCreated) _neighbourIndices.Dispose();

            _boidJobUnits = new NativeArray<BoidJobUnit>(boidData.UnitSpawnCount, Allocator.Persistent);
            _hashAndIndices = new NativeArray<HashAndIndex>(boidData.UnitSpawnCount, Allocator.Persistent);

            _neighbourIndices = new NativeArray<UnsafeList<int>>(boidData.UnitSpawnCount, Allocator.Persistent);

            for (var i = 0; i < _neighbourIndices.Length; i++)
                _neighbourIndices[i] = new UnsafeList<int>(8, Allocator.Persistent);
        }

        public void Update(BoidDataSO boidData, int activeCount)
        {
            if (!_boidJobUnits.IsCreated || activeCount == 0) return;

            var hashJob = new HashUnitJob
            {
                Units = _boidJobUnits,
                HashAndIndices = _hashAndIndices,
                CellSize = boidData.CellSize,
            };

            var hashJobHandle = hashJob.Schedule(activeCount, 64);

            var sortJob = new HashSortingJob
            {
                HashAndIndices = _hashAndIndices,
                ActiveCount = activeCount,
            };

            var sortJobHandle = sortJob.Schedule(hashJobHandle);

            var queryJob = new QueryJob
            {
                Units = _boidJobUnits,
                HashAndIndices = _hashAndIndices,
                CellSize = boidData.CellSize,
                NeighbourUnitsIndexes = _neighbourIndices,
                ActiveCount = activeCount,
            };

            var queryJobHandle = queryJob.Schedule(activeCount, 64, sortJobHandle);

            queryJobHandle.Complete();
        }

        public void Dispose()
        {
            if (_boidJobUnits.IsCreated) _boidJobUnits.Dispose();
            if (_hashAndIndices.IsCreated) _hashAndIndices.Dispose();
            if (_neighbourIndices.IsCreated) _neighbourIndices.Dispose();
        }

        private static int3 GetGridCell(float3 pos, float cellSize)
        {
            return new int3(math.floor(pos / cellSize));
        }

        public static int GetHash(int3 pos)
        {
            unchecked
            {
                return pos.x * 15485863 ^ pos.y * 32452843 ^ pos.z * 49979687;
            }
        }


        [BurstCompile]
        private struct HashUnitJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BoidJobUnit> Units;
            public NativeArray<HashAndIndex> HashAndIndices;
            public float CellSize;

            public void Execute(int index)
            {
                var jobUnit = Units[index];
                var gridCellPos = GetGridCell(jobUnit.Position, CellSize);
                var hash = GetHash(gridCellPos);

                HashAndIndices[index] = new HashAndIndex { Hash = hash, Index = index };
            }
        }

        [BurstCompile]
        private struct HashSortingJob : IJob
        {
            public NativeArray<HashAndIndex> HashAndIndices;
            public int ActiveCount;

            public void Execute()
            {
                HashAndIndices.GetSubArray(0, ActiveCount).Sort();
            }
        }

        [BurstCompile]
        private struct QueryJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BoidJobUnit> Units;
            [ReadOnly] public NativeArray<HashAndIndex> HashAndIndices;
            public float CellSize;
            public NativeArray<UnsafeList<int>> NeighbourUnitsIndexes;
            public int ActiveCount;

            public void Execute(int index)
            {
                var newList = NeighbourUnitsIndexes[index];
                newList.Clear();

                var centerGrid = GetGridCell(Units[index].Position, CellSize);

                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        for (var z = -1; z <= 1; z++)
                        {
                            var gridPos = new int3(x + centerGrid.x, y + centerGrid.y, z + centerGrid.z);
                            var hash = GetHash(gridPos);

                            var startIndex = BinarySearch(HashAndIndices, hash);

                            if (startIndex < 0) continue;

                            for (var i = startIndex; i < ActiveCount; i++)
                            {
                                if (HashAndIndices[i].Hash != hash) break; // sorted, first mismatch = done

                                var unitIndex = HashAndIndices[i].Index;

                                newList.Add(unitIndex);
                            }
                        }
                    }
                }

                NeighbourUnitsIndexes[index] = newList;
            }

            private int BinarySearch(NativeArray<HashAndIndex> array, int hash)
            {
                var left = 0;
                var right = ActiveCount - 1; // search only active range
                var result = -1;

                while (left <= right)
                {
                    var mid = left + (right - left) / 2;
                    int midHash = array[mid].Hash;

                    if (midHash == hash)
                    {
                        result = mid;
                        right = mid - 1;
                    }
                    else if (midHash < hash)
                    {
                        left = mid + 1;
                    }
                    else
                    {
                        right = mid - 1;
                    }
                }

                return result;
            }
        }
    }
}