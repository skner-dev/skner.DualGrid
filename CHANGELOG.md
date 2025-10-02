# Changelog

## [2.0.4] - 2025-10-02

- Optimized RenderTilemap refresh callbacks 
- Updated samples with more visually cohesive tileset textures
- Updated DualGridTilemap visual handles to react to tilemap's scale

## [2.0.3] - 2025-05-24

- Fixed issue [#7](https://github.com/skner-dev/DualGrid/issues/7): restricted transform component not using default inspector view

## [2.0.2] - 2025-02-17

- Removed unnecessary usings which could cause compilation issues with some users

## [2.0.1] - 2025-02-09

- Fixed a build issue caused by misusage of EditorPreviewTiles outside of the Unity Editor runtime

## [2.0.0] - 2025-01-17

- Introduced Dual Grid Tilemap Preview
    - A preview of any Dual Grid Rule Tile is available in its inspector window
    - It provides real-time visual feedback of any changes made to the Dual Grid Rule Tile
- Introduced previewing when editing a Dual Grid Tilemap
    - A tile preview is now visible in Scene View when using tilemap tools to edit the tilemap
- Added Unity Undo Api integration will all (or most) Dual Grid interactions
- Improved Inspector Behavior for Dual Grid Rule Tiles
    - Added the original texture to Dual Grid Rule Tiles
    - Added buttons to help with managing Dual Grid Rule Tiles
    - Added special inspector view when Dual Grid Rule Tile is not yet created
    - Improved Tiling Rules List
    - Improved inspector labels and tooltips
- Added multi object editing for Dual Grid Rule Tiles and Dual Grid Tilemap Modules
- Removed dependency on tile palettes
    - It's now possible to paint Dual Grid Tilemaps without setting up a placeholder tile and tile palette.
- Added automatic tilemap collider configuration:
    - If the collider type is Sprite, the TilemapCollider2D is set in the Render Tilemap
    - If the collider type is Grid, the TilemapCollider2D is set in the Data Tilemap
- Added support for GameObjects in the Dual Grid Tilemap
    - It's possible to set GameObject for the DataTile and associated RenderTiles
    - The Dual Grid Tilemap Module controls what GameObjects are used
- Introduced visualization Handles for Dual Grid Tilemaps
- Fixed issue with Unity's Tile lifecycle that would cause some _more_ corners to be malformed
- Fixed an issue with unhandled exceptions when no neighbour rule was defined
- Removed most restrictions when handling Tilemaps and TilemapRenderers
- Added compatibility with Unity 2021.3

## [1.0.3] - 2025-01-03

- Fixed issue [#5](https://github.com/skner-dev/DualGrid/issues/5): malformed bottom left corner render tile

## [1.0.2] - 2024-12-11

- Fixed build issue due to UnityEditor being used during runtime

## [1.0.1] - 2024-11-14

- Downgraded Unity 2D Tilemap Extras dependency from version 4.0.2 to 3.1.2
- Fixed issues with Samples project

## [1.0.0] - 2024-11-06

- Initial package release

## Semantic Versioning

This project follows Semantic Versioning (SemVer) for version numbering:

- **MAJOR version** for incompatible API changes,
- **MINOR version** for backward-compatible new features,
- **PATCH version** for backward-compatible bug fixes.

Additional pre-release and build metadata may be used when necessary.
