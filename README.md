# Interactive Water System  

A fully interactive water system designed for 2D and 2.5D games (works for 3D too, but some features are quite redundant).

## Features
- [GPU-driven ambient surface waves using Unity Shader Graph.](#ambient-surface-waves)  
- [GPU-driven and real-time contact ripples using Unity Shader.](#contact-ripples)  
- [Fully configurable caustics and distortion using Unity Shader Graph.](#caustics-and-distortions)  
- [Real-time planar reflections.](#planar-reflection)  
- [Fully compatible with the Unity 2D Light System.](#compatible-with-2d-light-system)  
- [Accurate depth clipping with both sprites and meshes.](#accurate-depth-clipping)  

## Demo
There are 2 demo scenes included in the project, Demo_WaterOnly and Demo_WithLighting. This [video](https://youtu.be/nht-2tldh_Q) and this [live demo](https://spookyfish.itch.io/interactive-water-system) are from Demo_WithLighting.

## How it works
The water system creates 2 meshes: 
- A "Front mesh" on the XY plane that is meant to be interpreted as the cross-section of a water body. The front mesh applies caustics and distortions to sprites behind it.
- A "Top mesh" on the XZ plane that is meant to be interpreted as the water surface. The top mesh has waves, ripples, and reflections.  

In the project files, you can find:
- The `CRT_AmbientWave` shader graph that renders ambient waves to the `AmbientWave` render texture.
- The `CRT_RippleSimulation` shader that renders all ripple interactions to the `RippleSimulation` render texture. This texture is run on demand in the `FixedUpdate` loop of the [InteractiveWater](https://github.com/daothienphu/InteractiveWaterSystem/blob/main/Assets/InteractiveWaterSystem/Scripts/InteractiveWater.cs) script.
- The [SimplePlanarReflection.cs](https://github.com/daothienphu/InteractiveWaterSystem/blob/main/Assets/InteractiveWaterSystem/Scripts/SimplePlanarReflection.cs) script that renders the reflections of reflectable sprites to the `PlanarReflection` render texture.
- The `FrontMesh` shader graph that uses the `AmbientWave` and `RippleSimulation` textures for vertex displacement, and applies caustics and distortions to sprites behind it based on sorting layer order.
- The `TopMesh` shader graph that uses the `AmbientWave` and `RippleSimulation` textures for vertex displacement, and the `PlanarReflection` texture for reflections.

## Configuration Requirements
The system needs the following configuration requirements that are not visible within the WaterSystem folder but still exist somewhere in the project:
- 2 additional layers: `WaterSystem_Water` (you can use the default `Water` layer) and `WaterSystem_Reflections`. Any GameObject with an attached SpriteRenderer component and layer set to `WaterSystem_Reflections` will appear in the reflection texture. This also works for Light2D lighting, although I prefer a single, dark color for my reflections. The `WaterSystem_Water` layer is only used for creating surface ripples from mouse input (which requires raycasting to a specific layer).  
![image](https://github.com/user-attachments/assets/1b068069-a017-44c5-92f2-9dc1849dbe8b)

- 2 additional sorting layers: `WaterTopMesh` and `WaterFrontMesh`. `WaterTopMesh` is used to mark where the Camera Sorting Layer Texture should stop. `WaterFrontMesh` can be replaced by any sorting layer that comes after `WaterTopMesh`. The other layers are for my own demo scene setup and are not required. Please note that the `Default` sorting layer should always come before the required sorting layers.  
![image](https://github.com/user-attachments/assets/6aad2f58-4273-4b65-8a87-d253c3cb623f)

- A custom Renderer2D with `Camera Sorting Layer Texture` enabled and set to `WaterTopMesh`. This is necessary for getting everything behind the front mesh to apply caustics and distortions on top of them.   
![image](https://github.com/user-attachments/assets/61c4d470-7984-4433-9b5c-d77740b35774)

- You also need a Global Volume for the 2D lights to show up. I forget this quite often.  

## Ambient Surface Waves
- Made in Shader Graph and outputs to a Custom Render Texture.
- Contains 3 layers of sinusoidal waves, with multiple configuration parameters.
![Unity_wD8PDAoi3s](https://github.com/user-attachments/assets/c10eb81c-4c3c-4f5c-a34a-5a902e6926ed)
- "That's a blatant lie! I saw 3 CustomFunction nodes!", I only use it to shorten this whole thing:
![Unity_vQfqjSVC25](https://github.com/user-attachments/assets/4c06dd60-7d48-4dc8-b13b-45e384853acd)

## Contact Ripples
- Made with a shader, the ripples need to differentiate between when a contact happens and normal simulation, which can be interpreted as different shader passes, which Shader Graph doesn't support, hence the use of a good ol' shader. I'm planning on converting it to Shader Graph somehow, so for the visual learners out there, come back in a few months, or years...
- Contact ripples can be created anywhere, with adjustable strength and initial direction (up/down).
- The initial shape of the ripple is currently set to a very small square, but it can be changed in code.
![Unity_PVy3xSyfYj](https://github.com/user-attachments/assets/04aac7ae-3f79-4ed9-9ede-f8dabeabc1b7)

## Caustics and Distortions  
- Made with Shader Graph, with many customizable parameters.
![Unity_joRE76Hy9f](https://github.com/user-attachments/assets/9ef34799-0432-4bcb-bac8-b1f3340ffd83)

## Planar Reflection
- Captures reflections with an additional camera, outputs to a Render Texture, with configurable resolution.
- The reflection texture is simply applied on top of the top mesh, which has yet to have correct surface normals after all the waves and ripples, making it a bit odd and static.  
![Unity_1CeBx0WVE4](https://github.com/user-attachments/assets/42e741f3-0d71-41fd-a026-3322ceee5a18)

## Compatible with 2D Light system
- The base shader for the top mesh and the front mesh is Sprite Lit.  
![Unity_q0ltGczV8p](https://github.com/user-attachments/assets/51a21fa6-84a4-4cd0-bc7e-66ea6d200573)

## Accurate depth clipping
- In Unity, all sprites are rendered on the Transparent queue, which means they aren't written to the depth buffer.
- The top mesh exists on the XZ plane and will clip into some sprites at some point. But since its base shader is Sprite Lit (for Light 2D compatibility), it doesn't have any depth data to clip into the sprites. Instead, the render order is based on the sorting order layer, meaning either a whole sprite is on top of or behind the water, no matter what the z-coordinate is.
- I initially solved this by attaching 2 materials to a sprite renderer, one unlit to make a "cutout" of the sprite on the water surface, then one lit to render the actual sprite.
- Then I realized while writing this that I could just make the sprite lit shader write to the depth buffer...
![Unity_OmkMP5c16n](https://github.com/user-attachments/assets/092c9dcf-7a5a-4f36-a838-14a82d24a6e1)

## Future improvements:
- Add the correct surface normals to fix static reflections.
- Caustics and Distortions are visually squashed and influenced by the waves. I'm gonna do something about that.
- The planar reflection is set to generate reflections based on a defined plane and normal, which contributes to the static feeling of the reflections, which needs improvement.
