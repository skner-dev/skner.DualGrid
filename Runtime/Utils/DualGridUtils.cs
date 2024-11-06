using System.Collections.Generic;
using UnityEngine;

namespace skner.DualGrid.Utils
{
    public static class DualGridUtils
    {

        /// <summary>
        /// Gets the 4 render tile positions from a <paramref name="dataTilePosition"/>.
        /// </summary>
        /// <remarks>
        /// Assumes that the render tilemap offset is always (-0.5, -0.5).
        /// </remarks>
        /// <param name="dataTilePosition"></param>
        /// <returns></returns>
        public static Vector3Int[] GetRenderTilePositions(Vector3Int dataTilePosition)
        {
            return new Vector3Int[]
            {
                dataTilePosition + new Vector3Int(0, 0, 0),
                dataTilePosition + new Vector3Int(1, 0, 0),
                dataTilePosition + new Vector3Int(0, 1, 0),
                dataTilePosition + new Vector3Int(1, 1, 0)
            };
        }

        /// <summary>
        /// Gets the 4 data tile positions from a <paramref name="renderTilePosition"/>.
        /// </summary>
        /// <remarks>
        /// Assumes that the render tilemap offset is always (-0.5, -0.5).
        /// </remarks>
        /// <param name="renderTilePosition"></param>
        /// <returns></returns>
        public static Vector3Int[] GetDataTilePositions(Vector3Int renderTilePosition)
        {
            return new Vector3Int[]
            {
                renderTilePosition - new Vector3Int(0, 0, 0),
                renderTilePosition - new Vector3Int(1, 0, 0),
                renderTilePosition - new Vector3Int(0, 1, 0),
                renderTilePosition - new Vector3Int(1, 1, 0)
            };
        }

        /// <summary>
        /// Gets all positions in a square range around a specified position.
        /// </summary>
        /// <remarks>
        /// Works in two dimensions only. z axis is unchanged.
        /// </remarks>
        /// <param name="centralPosition"></param>
        /// <param name="size"></param>
        /// <param name="includeCenter"></param>
        /// <returns></returns>
        public static List<Vector3Int> GetSurroundingPositions(Vector3Int centralPosition, int size, bool includeCenter = false)
        {
            var positions = new List<Vector3Int>();

            for (int y = -size; y <= size; y++)
            {
                for (int x = -size; x <= size; x++)
                {
                    if (!includeCenter && x == 0 && y == 0)
                        continue;

                    positions.Add(new Vector3Int(centralPosition.x + x, centralPosition.y + y, centralPosition.z));
                }
            }

            return positions;
        }

        /// <summary>
        /// Converts a valid render tile offset, used to offset a DataTilePosition into a RenderTilePosition,
        /// into a valid neighbor offset, used by Unity's Tilemap to calculate neighbors and apply rule tiles.
        /// </summary>
        /// <remarks>
        /// This is only valid for render tile offsets, aka, positive offsets. 
        /// Refer to 
        /// </remarks>
        /// <param name="renderTileOffset"></param>
        /// <returns></returns>
        public static Vector3Int ConvertRenderTileOffsetToNeighborOffset(Vector3Int renderTileOffset)
        {
            return new Vector3Int(
                renderTileOffset.x == 0 ? -1 : renderTileOffset.x,
                renderTileOffset.y == 0 ? -1 : renderTileOffset.y,
                renderTileOffset.z
            );
        }

        /// <summary>
        /// Converts a valid data tile offset, used to offset a RenderTilePosition into a DataTilePosition,
        /// into a valid neighbor offset, used by Unity's Tilemap to calculate neighbors and apply rule tiles.
        /// </summary>
        /// <remarks>
        /// This is only valid for data tile offsets, aka, negative offsets. 
        /// Refer to 
        /// </remarks>
        /// <param name="dataTileOffset"></param>
        /// <returns></returns>
        public static Vector3Int ConvertDataTileOffsetToNeighborOffset(Vector3Int dataTileOffset)
        {
            return new Vector3Int(
                dataTileOffset.x == 0 ? 1 : dataTileOffset.x,
                dataTileOffset.y == 0 ? 1 : dataTileOffset.y,
                dataTileOffset.z
            );
        }

    }
}
