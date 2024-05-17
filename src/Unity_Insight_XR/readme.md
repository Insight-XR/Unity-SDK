Step 1: SDK Setup and Replay System Review

-Forked repository on GitHub.<br />
-Cloned repository on local machine.<br />
-Integrated SDK within Unity project.<br />
-Understanding of the replay system's functionality and implementation.<br />



https://github.com/norame1/Unity-SDK/assets/84630427/de40b27a-e417-4bbe-bbd2-ae82e03141e6

Step 2: Custom Scene Replay

-Custom scene created in Unity.<br />
-Replay feature successfully integrated and functional within the scene.<br />
-Tested and verified replay functionality.<br />



https://github.com/norame1/Unity-SDK/assets/84630427/8087fd67-6d75-4b64-b385-f49cb81089ee

Step 3: Data Manipulation and Heatmap Logic

In Step 3, I implemented the logic to process heatmap data using raycasting techniques and store focus areas within the scene.

-Raycasting for Data Capture:
    In the EmitHeatmapParticle() method, a ray is cast from the controller position to detect intersections with scene objects.
    If the ray hits an object, the hit position is recorded as a focus area for heatmap visualization.
-Data Processing and Storage:
    Upon detecting a hit, the hit position is stored and used to emit heatmap particles and spawn sphere marks.
    A dictionary sphereCounts is used to store the count of sphere marks at each focus area position.
-Heatmap Particle Emission:
    Heatmap particles are emitted at each focus area position using the SpawnHeatmapParticle() method.
    This method instantiates a particle at the hit position, contributing to the heatmap visualization.
-Sphere Mark Spawning:
    Sphere marks are spawned at the hit position on the intersected object to visually represent focus areas.


Step 3: Data Manipulation and Heatmap Logic

Objective:
Develop a system to process and visualize heatmap data.

Implementation:
In Step 3, I implemented the logic to process heatmap data using raycasting techniques and store focus areas within the scene.

Raycasting for Data Capture:
In the EmitHeatmapParticle() method, a ray is cast from the controller position to detect intersections with scene objects.
If the ray hits an object, the hit position is recorded as a focus area for heatmap visualization.
Data Processing and Storage:
Upon detecting a hit, the hit position is stored and used to emit heatmap particles and spawn sphere marks.
A dictionary sphereCounts is used to store the count of sphere marks at each focus area position.
Heatmap Particle Emission:
Heatmap particles are emitted at each focus area position using the SpawnHeatmapParticle() method.
This method instantiates a particle at the hit position, contributing to the heatmap visualization.
Sphere Mark Spawning:
Sphere marks are spawned at the hit position on the intersected object to visually represent focus areas.
The spawned spheres are stored in the sphereCounts dictionary for color adjustment based on count.
Deliverables:

Implemented raycasting to detect focus areas.
Developed logic to process and store heatmap data.
Created methods to emit heatmap particles and spawn sphere marks.


Step 4: Heatmap Visualization Development (Particle System Approach)

For Step 4, I utilized Unity's particle system to visualize the heatmap within the scene.

-Particle System Setup:
  A reference to the heatmap particle system is provided in the script for easy access and configuration.
  The particle system properties, such as emission rate and color, are adjusted to create the desired heatmap effect.
-Particle Emission Based on Focus Areas:
  Heatmap particles are emitted at focus area positions detected through raycasting.
  The EmitHeatmapParticle() method triggers particle emission upon detecting focus areas.
-Sphere Mark Interaction:
  Sphere marks are spawned at focus area positions to visually represent heatmap data.
  The spawning of sphere marks also contributes to the visualization of focus areas within the scene.



https://github.com/norame1/Unity-SDK/assets/84630427/bcc9b8f6-e217-4374-8ad1-e4f78b02e9a6

