Hello! Thank you for the purchase. Hope the assets work well.
If they don't, please contact me slsovest@gmail.com.




Assembling the turrets:


All the parts may have containers for mounting other parts, their names start with "Mount_".
(In some cases they may be deep in bone hierarchy).
Just drop the part in the corresponding container, and It'll snap into place.

- Start with Base ("Base_Turret_" or "Base_Tower_"), find the "Mount_top" container inside it. 
- Drop the "Base_top" part into it, find another "Mount_top" container inside it.
- Drop the Shoulders or the Cockpit into the "Base_Top"
- Find the other mounting points inside the Cockpit or Shoulders


Which parts you can drop into which containers:
(The best way would be just to explore the turrets in the demo scene).

"Mount_Top": "Base_Top", "Top_...", "Tower_...", "Shoulders_", "Cockpit_..."
"Mount_Cockpit": Cockpits
"Mount_Backpack": Backpacks
"Mount_Weapon L/R": Weapon_...", "HalfShoulder_..."
You can try to drop the "Roof" parts into "Mount_Weapon_Top" containers inside the cockpits, but generally you'll just have to eyeball it.


After the assembly, the turrets consist of many separate parts and, even with batching, can produce high number of draw calls.
You may want to combine non-animated parts into a single mesh for the sake of optimization.

All the weapons contain locators at their barrel ends (named "Barrel_end", or "Barrel_end_[number]" in case there are multiple barrels).




Textures:


The source .PSD can be found in the "Materials" folder.
For a quick repaint, adjust the layers in the "COLOR" folders. 
You can drop your decals and textures (camouflage, for example) in the folder as well. Just be careful with texture seams.




If you have any ideas how I could organize the assets any better way, or if you're missing any particular animation or module, please, write (slsovest@gmail.com). 
Will try to include it into this pack or into the future ones.



Version 2.0 (March 2018):
Added flat-colored PBR textures.
Added new parts:
- fences (3 levels)
- top-mounted radars (3 levels)