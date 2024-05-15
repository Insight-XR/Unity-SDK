# Heatmap Visualizer Using Shader Approach

## Overview
This project implements a heatmap visualizer using the Shader approach. It creates a visualization of heatmaps in a scene resembling a school class with benches, chairs, and a board. The scene includes a player character, and the project incorporates replay logic and even does the dashboard uploads.

## Approach
Two main approaches are used to visualize the heatmap:

1. **Raycast Method:**
   - The `RaycastVisualizer` script sends a ray to the mouse position.
   - Upon hitting objects tagged with "HeatMapLayer," a shader is applied to the hit object to visualize the heatmap.

2. **Projectile Method:**
   - The `ProjectileScript` the movement of a projectile game object positive z direction.
   - The `Projector` is attached to the main camera and projects the projectile GameObject from the player.
   - The `ProjectorVisualizer` script is used to visualize the heatmap when the projectile hits objects tagged with "HeatMapLayer." The shader is applied to generate the heatmap.

## Scripts
1. `RaycastVisualizer`: Handles raycasting to visualize the heatmap.
2. `ProjectileScript`: Moves the projectile game object in z direction.
3. `ProjectorVisualizer`: Visualizes the heatmap using the projector method.
4. `Projector`: Projects the projectile GameObject from the player's Main Camera. 
## Shader
1. `HeatMapShader`: A shader is used to apply visual effects to objects hit by either the raycast or projectile method. Gives a HeatMap Gradient.

## Projector Visualizer
https://github.com/samarth0104/Unity-SDK/assets/144517774/a09b83e8-7ee9-44cc-9503-647c1945804e

## Raycast Visaulizer
https://github.com/samarth0104/Unity-SDK/assets/144517774/3e2f9b30-041c-4c6c-a274-fa2fa5266fd6




