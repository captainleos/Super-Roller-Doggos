# 2.3.3

- WorldTools - Added Layer Based Filtering feature for all layers to detect underlying models and filter placement based on their layers
- WorldTools - Duplicate Layer option to create a copy of the layer object in scene
- Linux platform is now fully supported and tested
- Removed World Finalization Recompile operation
- Bugfix: Post Process Volume's "Is Global" checkbox is now set properly so that Post Processing is back in scenes globally
- Bugfix: Satellite images' resolutions are now set properly
- Bugfix: Background Terrain is now guaranteed to be generated and settings are now properly set
- Bugfix: Removed Auto Installer conflict with Unity Visual Scripting package
- Few minor bugfixes and improvements based on user reports


# 2.3.0

- Added "Savannah Mini Game" demo scene with latest features of PLAYER tab, new Time of Day cycles & Grass Touch Bending
- Revamped "Time of Day" component with new optimized and high performance Day/Night Cycle
- Fully real-time lighting, shadows, reflections and ambient lighting in a performant manner
- Added Touch Bending feature for grass, plant and tree leaves interacting with assigned bending colliders
- Production-Ready Runtime Spawner to procedurally place interactable objects such as NPCs around player
- Added Threading support on WebGL platform so that grass layers will be rendered in WebGL builds
- TerraFormer - Revamped Procedural Puddles with better distribution on terrain surface
- TerraFormer - Added GPU Simplex Noise function so that all effects will use this instead of noise textures
- TerraFormer - Fixed bug where terrain holes were not working due to Max Sampler Registers
- TerraFormer - Removed all bugs and warnings which was failing builds in latest Unity versions
- WorldTools - bug fixes for grass mask data, GPU layer Undo, Texture Exclusions
- WorldTools - Added UNDO operations switch for faster world editing
- Layer Editing Tools - Convert layer's maskdata to image file and vice versa to feed in user-customized mask images
- Layer Editing Tools - Added UNDO operations switch for faster world editing
- TERRACE node - fixed bug for vertical sliders to define terrace positions
- VFX tab - Day Night Cycle now has the option "Auto Lightmapping Controls" so that users can decide on Lighting Baking options
- Lots of bug fixes and improvements based on user reports


# 2.24.0

- Introducing "Dynamic Player Interactions" in new "Player" tab
	Automatically converts GPU models to intractable objects around any static or dynamic target such as camera, main character
	or NPC to interact with their physics, collisions, scripts, Runtime NavMesh and any other kinds of interactions as you
	would with a normal gameobject
- Introducing "World Tools" to sync and edit all GPU and Grass layers from one place using editing brush and global settings
- WorldTools "Live Sync" - Adopt all layers when terrain heightmap or textures change while importing/editing heightmap or
	painting textures using Unity’s built-in terrain editing tools
- TerraFormer (Terrain Shader): Implemented Add Pass shader which means you can now have 8 terrain layers/textures on world
	terrain
- TerraFormer (Terrain Shader): Revamped Procedural Puddles feature with proper scattering and rendering
- Save all World Resources (e.g. materials, textures...) in an exclusive folder for each scene/world generation so you can have
	multiple scenes in a project each with their specific resources and settings
- Demo Scene: New demo scene has been produced with the latest features as an editable scene using new "World Tools" component
	on "TerraWorld" gameobject in scene
- Demo Scene: Users can throw physics balls in scene to check with new "Dynamic Player Interactions" feature where all
	surrounding models have collision detection now
- Revamped all included Art Content to sync with new features and improved their LODGroups plus adding colliders to solid/static
	models to be usable with new "Dynamic Player Interactions" feature
- GPU Instance Rendering: Faster overall rendering plus introducing Real-time Occlusion Culling system against any occluder
	objects in scene
- Better Clouds rendering & scattering
- WorldTools/LayerTools – Added Undo operations for brush editor paintings and all UI actions
- Scatter Layers - Removed mask images from physical resources and replaced them with serializable data inside layers for faster
	loading/unloading of world data
- Added forums to our site so that users can access all art and miscellaneous content from there
- Auto Installer System Revamped.
- Bug Fix: Removed Dropbox link downloads due to server limits and replaced with our own servers
- Better Lake/River/Ocean detection from landcover data and generation
- Better "Biome Type Filter" output masks
- Many reported bug-fixes and improvements have been applied for this version


# 2.13.3 ( Nightly Update )

- Templates now are available on multi servers
- Some bugs for Water-Generator have been fixed


# 2.13.2 

- Build Issue has been fixed
- Some bugs have been fixed


# 2.13.1 

- Non-Essential modules and assets will be downloaded from the internet on demand
- Project import time increased dramatically
- Passed stress-tests for Linux & Mac users
- Demo scene will be downloaded from internet on demand
- Templates will be fetched from the internet on demand
- Dark/Light Theme modes supporting latest Unity's professinal skin
- Auto Installer revamped
- Grass Scatter - Option to raycast against underlying surface and does layer-based placement
- Export package botton added to support sharing graphs between users
- Atmospheric Scattering effect is fixed for Linux & Mac users
- Volumetric Fog effect is fixed for Linux & Mac users
- Namespaces were reviewed
- Tint color for Main Terrain and Background Terrain surfaces are supported now
- Automatically removes invisible or under terrain objects at the end of world generation
- Decrease snow texture resources resolutions 
- "Fly Cam" and "Editor Cam Sync" features are exposed in the Settings tab
- "DOF" effect feature is added to the default post processing profile
- Terra_Standard shader now supports "Procedural Cover layer" for each object respectively
- Background Terrain now supports Procedural Snow
- Progress bars updated to support Unity 2020.x cycle
- Reported bugs have been fixed


# 2.12.4 ( Nightly Update )

- NameSpaces (PostEffectsBase @ TriangleNET) conflicts has been fixed.
- "Patch Update Window" bug fixed.


# 2.12.3 ( Nightly Update )

- Elevation data decoder updated.
- Linux elevation decoder bug fixed .
- "Best Resolution" Option bug fixed.


# 2.12.2 ( Nightly Update )

- Expose Water Angle for terrain deformation in SETTINGS tab as a global parameter
- Water shader and scripts in TerraWorld - Change shader path name and take out from UnityStandardAseets namespace in all water related scripts
- Mask Editing - Update layer when changing mask in inspector
- TerraMesh Node - Set defaults
- Brought back CleanUp function after generation.
- Remove "Global Power" parameter from COLORMAP BLENDING in Terrain tab


# 2.12.1 ( Nightly Update )

- Project Launcher - Remove Mac tab
- TResourceManager - LoadCloudsResources check for both .fbx & .FBX files
- Wind, Snow & Flat Shading does work in builds
- Remove Mouse Cursor in builds via ExtendedFlyCam


# 2.12

- Unity 2020.x compatibility
- Mac/Linux systems: Automatically search for Mono config file and remove gdiplus path references
- Removed Mac/Linux Instructions tab from the Project Launcher as no manual instrcutions are needed for these platforms
- Added "Load Demo Scene" checkbox option in Project Launcher so that users can select to whether load demo scene automatically or not
- Update csc.rsp & mcs.rsp files to properly reference System.Drawing.DLL from Unity's Mono folders to avoid any DLL conflicts
- Removed Removed unnecessary DLLs from project
- Fixed SceneView UI in Unity 2019.1 & later
- Some bugs fixed


# 2.11

- Mac/Linux systems: No more manual instrcutions needed to install and fully compatible on these platforms
- Removed Cinemachine from Auto Unpacker as no longer needed
- Fix: Edge Smoothnes (DX11 Phong Smoothing) is now 0 to match collisions and placement with terrain collider
- Updated all templates with new changes
- Removed water reflections by default on all templates
- Some bugs fixed


# 2.0

- Biomes - Added "Grass Scatter" node to place dense vegetation suited for grass, flower and plant rendering
- Templates - Added 3 new graph Templates of "Minecraft", "African Savanna" & "Apocalypto"
- Templates - Updated "Forest" & "GrassLands" templates with the new Grass Rendering System
- Scenes - Updated demo scenes with new features in place
- TerraFormer - Added "Procedural Puddles" feature on terrain surface
- Scatter Layers - Updated Editing tools with more precise mask painting on layer parents in the hierarchy
- Lighting - Increased lighting range in scenes for full HDR rednering
- Art Assets - Added plenty of our in-house models/textures/materials to use in built-in templates and user graphs
- Heightmap - Added "Voxelize Heightmap" processor to generate cubic heights on terrain inspired by Minecraft
- Heightmap - Unlocked 4K resolution terrains which was previously capped at 2048
- Heightmap - Added "Elevation Exaggeration" option in "Heightmap Source" node to increase/decrease terrain heights and adopt world elements based on that
- Clouds - Added custom "mesh" & "color" options in Clouds settings
- Horizon Fog - Updated color blending on scene's horizon level and sync with Atmospheric Scattering effect
- TerraFormer (Terrain Shader) - Filter "Snow" & "Puddles" distribution under water areas on terrain surface
- GPU Instance Scatter node - Added "Check Bounding Box" option to guarantee placement based on model bounds instead of pivot only checking
- GPU Instance Scatter node - Added "Frustum Multiplier" option to increase/decrease camera frustum checking for offscreen items in the world
- Sun - Better sun effects combined with crepuscular rays
- Builds - Tested all TerraWorld features and working out of the box
- Tested and fully compatible on latest Unity 2019.4 LTS version
- Some bugs fixed


# 1.16

- Editor tools have been added for GUP Instances, Lakes, Seas & Rivers plus all procedural mesh objects
- Heights of created objects are equal to real world heights now
- Some bugs fixed


# 1.15

- Added Volumetric Fog effect in VFX tab
- Revamped GPU Instanced Rendering with magnitudes of better performance (especially in larger scenes) with zero memory footprint
- Support for Unity's built-in LODGroup component on prefabs used for GPU Instanced Rendering
- Softer and more organic coastlines for lakes and seas
- "Biome Type Filer" node has been revamped to extract precise landcover data for different biome types
- Better performance and accuracy for landcover data extraction and analysis
- Better performance and accuracy for "Water Generator" node
- Better performance and accuracy for "Mesh Generator" node
- Better memory management and low footprint
- Better lighting for day & night time
- Updated scenes with latest features
- Updated templates with latest features
- Some bugs fixed


# 1.14

- Added Auto installer (No manuall install steps needed anymore)
- Unity 2019.3 UI Revamp with Flat Design
- Updated Templates
- Area Size limit changed
- TerraFormer (Terrain Shader): Support for terrain holes in 2019.3 and later
- TerraFormer (Terrain Shader): Fixed bug for normalmaps in different Unity versions
- World offset at the beginning is removed now and world is generating at origin at start
- Clouds: Automatically sets cloud particles' Motion Vector settings based on Flat Shading to avoid blurry clouds in LowPoly worlds
- Clouds: Fixed black particles at the borders of rendering distance
- Some internal bug fixes


# 1.13

- Added Low-Poly style templates
- Added support for ocean/sea and big water areas generation obtained from Landcover data
- Added support for island generation surrounded by big water areas such as oceans, seas and big lakes
- Revamped River/Lake generation and underlying terrain deformation with higher accuracy and more realistic blending between water mesh and terrain surface
- Updated all Realistic/Stylish templates with the new core features and cleaner graph interface
- Updated Demo Scenes and their graphs with new features in TerraWorld
- VFX: Added Poly Style section which takes the entire scene elements to Flat Shading (Low-Poly) rendering. It automatically converts materials to Low-Poly if using TerraFormer on terrain, Terra_Standard on objects and built-in clouds
- VFX: Added God Rays (Crepuscular) option for more realistic sun rendering and Rayleigh Scattering
- VFX: Horizon Fog now supports user-defined sky/fog color suited for Stylish/Phantasy Scene Moods ("Stylish Night" demo scene using it now)
- VFX: Day/Night Cycle controls can be turned off now for 3rd party Directional Light (sun/sky) controllers
- VFX: Moved Background Terrain's Snow Cover option into Procedural Snow section
- TerraFormer (Terrain Shader) now comes with another variant which supports Instanced Drawing but with DX11 Tessellation disabled suited for mobile/VR platforms (All Stylish templates and "Stylish Night" demo scene using it now)
- TERRA_STANDARD: All materials with Terra_Standard shader can have their features exclusively on/off per material so that global settings will follow their states (e.g. a material with snow disabled will never receive global snow controls unless this feature is activated on material)
- TERRA_STANDARD: Updated Wind Simulation with more realistic vertex animations for wind
- TERRA_STANDARD: Casts proper animated shadows based on wind animations now which was previously static and didn't follow vertex animations
- TERRAIN: Directly switch terrain layers by dragging & dropping terrain layer assets in Terrain tab UI
- TERRAIN: Moved "Splatmaps", "Terrain" & "Background Terrain" sections from Global Settings tab into Terrain tab
- TERRAIN: Terrain materials (High-End/Legacy) will be properly set now for Unity 2019.x and up
- LANDCOVER: Revamped Landcover processor methods to have much faster data parsing
- LANDCOVER: Revamped Mask generator methods to bring extremely faster mask generation passing between nodes
- Bug Fix: Global Speed in Day/Night Cycle was failing occasionally
- Bug Fix: "Best Resolution" in Heightmap Source node now returns the highest resolution from server independently from Imagery resolution maxed at 2048 (Will be unlocked to 8192 later when heightmap processors ported to compute shaders)
- PatchRenderer of GPU Instancing has better performance now
- New Templates UI design for better-categorized selection
- The world will be generated immediately after loading templates/graphs now
- Auto Generate lightmaps will be disabled when the Auto Day/Night cycle is enabled
- Added Scene Graph Loader which optionally loads the scene's corresponding graph upon scene loading automatically
- Added progress bar at the center of the screen while in the progress of world generation so that user will know the progress even when Unity window is not focused
- This version of TerraWorld is now entitled as "Pro" in favor of the upcoming Lite version with core functionalities only
- A lot of major and minor bug fixes


# 1.0.0215

- Bug Fix: Lerc64 DllNotFoundException errors
- Removed ProjectFileHook.cs which has no dependency now
- Fixed black screen when in Forward Rendering and TerraWorld window loads
- Added InputManager.asset next to the scene files in order to retrieve character controls set through Project Settings' Input section


# 1.0.0205

- Bug Fix: All connection and network errors
- Better Progress Bar for world generation
- Added Runtime Spawners for GPU Instances, GameObjects & timed FX (Beta)
- Added "Stylish Night" demo scene
- Added Package Maker for graphs so that users can share graphs with all embedded resources via unitypackages
- Added Crepuscular Rays for sun rendering (Experimental)
- Added Stylish Template's graph and resources
- Added "Ellen" character from Unity's "3D Game Kit" for exploring scenes
- Added CineMachine camera control for demo scenes
- Removed grass patches from "TerraWorld Scene" demo to be replaced by new grass rendering solution (WIP)
- Better horizon falloff color

