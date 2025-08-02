Thanks for checking out the WaypointSystem asset!

Inside the Demo folder you’ll find two UnityPackage files. Although there isn’t a built-in demo for each pipeline, the asset’s core code works normally once you set it up.

What’s in Demo?

• DemoURP_source.unitypackage — A small demo scene for the Universal Render Pipeline (URP). Import this to get three waypoints, a simple movement script, a grass texture, and a skybox configured for URP.

• DemoHDRP_source.unitypackage — A small demo scene for the High Definition Render Pipeline (HDRP). Import this to get the same three-waypoint setup, movement code, grass, and skybox optimized for HDRP.

Why separate packages?

URP and HDRP use different materials, settings, and assets, so they can’t share the exact same scene file. By keeping the demo scenes in their own UnityPackage files, we avoid compatibility issues.

Bottom line: you only need to import the demo package that matches your render pipeline. The core scripts, prefabs, and markers themselves work normally in any pipeline.

Quick Start:
1. Open your Unity project.
2. In the Project window, go to Assets/WaypointSystem/Demo.
3. Double-click DemoURP_source.unitypackage or DemoHDRP_source.unitypackage (depending on your pipeline) to import.
4. Open the imported demo scene (e.g., WaypointSystem_Demo_URP.unity or WaypointSystem_Demo_HDRP.unity).
5. Press Play and try out the waypoints!

Happy coding!