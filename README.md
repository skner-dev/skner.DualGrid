# Dual Grid

Welcome to the repository of my Dual Grid implementation for Unity.

## Features

Here's what this library can do:

- 🔥 __Fully working Dual Grid System__: Paint a Data Tilemap and it all gets visually updated in a Render Tilemap.
- 🔄 __Automatic Setup__: It takes seconds to setup a dual grid system;
- 🤝 __Full Unity Integration__: Full integration with Unity's Tilemap and Rule Tiles;
- 🧩 __Rule Tile Creation__: Streamlined rule tile creation with full rule definition;

## The Dual Grid Tilemap

The Dual Grid Tilemap is a method to render tiles using two grids, instead of one. One grid is to indicate what each tile is and the render tilemap renders tiles with a half-tile offset in both axis. 

Many people use different terms for each grid, but in this library you'll find the __Data Tilemap__ and the __Render Tilemap__.

These are the big reasons (in my view) why using this approach is powerful:

- 🚀 __Scalability__: Create all tilemap combinations with only 16 tiles, saving a lot of work.
- ✨ __Corner design flexibility__: Fully round corners not possible with traditional tilemap structures.

I won't go into detail here, because this concept is already very well explained:

- [Jess Hammer's Youtube Dual-Grid Tutorial](https://youtu.be/jEWFSv3ivTg)
- [Oskar Stålberg's Twitter Post](https://x.com/OskSta/status/1448248658865049605)
- [ThinMatrix's Terrain Generation Devlog](https://youtu.be/buKQjkad2I0?t=233)

## Getting started

To start using this library, check the [installation guide](Documentation~/installation-guide.md). It has a guide on how to __install__ and __setup__ the library with a detailed explanation.

To start using it, I recommend the [user guide](Documentation~/user-guide.md). It's a page with an article on how to use the dual grid with detailed explanations.

If you're looking for quick step-by-step usage instructions, there's also a [cheatsheet](Documentation~/cheatsheet.md).

## Dependencies

This library depends on [Unity.2D.Tilemap.Extras](https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@4.1/manual/index.html). It was tested using version 4.0.2, which comes pre-built when a new 2D Universal Render Pipeline Project is created.

This package is compatible with Unity 2022.1+.

## Author's note

I knew about the Dual Grid system before, but seeing [Jess Hammer's implementation](https://github.com/jess-hammer/dual-grid-tilemap-system-unity) inspired me to use my programming abilities to make my own implementation, as I couldn't find more anywhere. 

Everything is open source and I'll be releasing it in the Unity Asset Store __for free__ so anyone can readily use it without having to get it from Git. I'm making this available now because Unity Asset Store submissions are excruciatingly long.

You can reach me at sknerdev@outlook.com with suggestions or feedback. 

Thanks for reading.
