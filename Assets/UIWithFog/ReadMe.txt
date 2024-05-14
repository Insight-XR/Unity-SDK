UI With Fog

Default UI shaders does not takes fog into account on rendering, but in WorldSpace mode it's can be necessary. 
I've made shader to draw UI elements with fog and that is modified UI/Default shader code. So it's will works 
fine in most Unity versions and also with URP.

Package contains UI/Default_Fog shader to work with Global Fog and example DEMO scene. Elements rendered with it 
correctly drown in fog if moving away from the camera. Works fine in both Standard and Universal Render Pipelines.

How to use the shader:
1. Crete new material in any default way (example: Right click on Assets at the Project Tab [Create] -> [Material]);
2. Select shader UI/Default_Fog for a new material at the Inspector Tab;
3. Select New Material on all UI elements at its Inspector Tab to set up rendering with fog.