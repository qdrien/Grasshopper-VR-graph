# eNTERFACE-graph-architecture

Version of Unity used: 2018.1.9f2

Goals:    
- Bring GH graphs into VR (e.g. table metaphor)
  - What is the optimal size/height/slope of the table?
  - What about the font?
- Capitalize on the 3rd dimension for an alternative visualization (e.g. z axis linked to how deep we are in the graph)
  - Related: since we probably cannot have as much information as in a desktop-based tool (i.e. a smaller part of the graph can be seen in VR compared to GH), we can try to use the 3rd dimension to compensate (e.g. stack ports upwards to make components smaller)
- Enable graph editing and try different interaction options for the following actions:
  - Add a component (a node in the graph)
  - Remove a component
  - Move a component
  - Add a link (an edge in the graph)
  - Remove a link
  - Move the viewpoint (what part of the graph we currently see)
  - Scale the viewpoint (zoom in/out in the graph)
  - Add/remove/"resize" group?
  - Modify parameter values (toggle, slider, panel)
- For controller-based interaction, should try with different controller models (e.g. something like a stick would be easier to handle for touching/grabbing things)
