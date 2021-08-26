# Redesign changes:

Made a clean separation between Visualizations package and ROS-TCP-Connector. Now ROS-TCP-Connector only provides general purpose interfaces for subscribing to messages and receiving topic updates, plus a generic hud that offers tabs and windows that can be used for any purpose.
Message Visualizations provides all the specific visualization logic.

Each IVisual no longer represents a single message, but a single topic; as new messages are sent to it, most IVisuals will throw away the old message and visualize the new one, but the exact behaviour is up to the IVisual itself.

The IVisualFactory interface changed from CreateVisual(Message) to GetOrCreateVisual(topic), reflecting that each visual is linked to a topic rather than a message. And the the "GetOr" reflects that the factory is now responsible for keeping hold of the IVisuals it has created. We now have a clear "official" way to get hold of the visual for a given topic.

# Message Visualizations design

The goal of this package is to provide a toolkit for users to visualize messages in whatever way suits their needs:
  * We have made visualizations for all the ROS common message types, exposing settings so that users can adjust their color, size, etc; or they can completely replace them with their own visualizations.
  * Users can also make their own visualizations of their custom message types. We've tried to make it easy to call out to our built-in visualizations, because custom messages often contain ROS common messages as elements.

To this end, we provide multiple layers of configurability.

First and foremost, the set of visualizers to use is entirely up to the user: each visualizer is a MonoBehaviour component that lives in the Unity scene. (To get them started, we've made a prefab they can just drag into the scene).
This gives the user a nice obvious workflow for editing visualizer settings (select the relevant component, use the inspector to edit it), and it's also easy to understand how to add, remove, enable or disable visualizers as desired.

<img src="images~/VisualizationSuiteExample.PNG">

# Visualization Flow

<img src="images~/MsgVizFlow2.svg">

This diagram illustrates the important components and their roles at two key times - when ROS notifies us about a new topic ([OnNewTopic](https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/3772e3ad460a61d0e3ffd097ba10fa9e5f147e55/com.unity.robotics.message-visualizations/Runtime/Scripts/VisualizationTopicsTab.cs#L32)), and when a message is actually sent or received. ([UpdateVisual](https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/3772e3ad460a61d0e3ffd097ba10fa9e5f147e55/com.unity.robotics.message-visualizations/Runtime/Scripts/VisualizationTopicsTabEntry.cs#L115)).

1. The VisualizationTopicsTabEntry class, which has subscribed for updates from RosConnection, is notified that a message has been sent or received.

2. If necessary it creates a visual for this message, by calling out to the VisualFactoryRegistry which keeps track of all visualizers. Once a visualizer has been obtained, we have it create an IVisual for the given message. We cache the visual so that we can skip this step in future.

3. We add the new message to this visual, and it creates or updates any graphical, hud or other elements it deems necessary.

# VisualFactoryRegistry

https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/laurie/HudRefactor/com.unity.robotics.ros-tcp-connector/Runtime/MessageVisualization/VisualFactoryRegistry.cs
- This static class keeps track of what visualizers exist, and will pick a suitable visualizer for a given topic and message.
- Each visualizer is expected to register itself with the VisualFactoryRegistry at startup.
- There are two different ways to register a visualizer - by message type, or by topic.
    - Registering by topic takes priority: it means that your visualizer is responsible for all messages sent or received on that topic.
	- Registering by message type means that your visualizer will handle all messages of that type that don't have a specific topic visualizer.
- All our default visualizers have a "topic" field; if you provide one, they will register themselves by topic. If you leave it blank, they will register by message type.
- In addition to the topic/type separation, visualizers are registered with a "priority" value. All the default visualizers we have in the prefab are set to low priority, so we will use any default-priority visualizer in the scene.


# IVisualFactory vs IVisual

- Each visualizer implements the IVisualFactory interface. This is the type that gets registered in the VisualFactoryRegistry. IVisualFactory is responsible for creating IVisuals (as the name implies).
- The main function in IVisualFactory is `IVisual GetOrCreateVisual(string topic)`. As that function signature implies, each IVisual represents the visualization of one specific topic.
- There can be many IVisuals active at once. The key idea here is that a single visualizer is not necessarily responsible for just one visualization at a time. For example, maybe we have only one visualizer for Point messages, but it might be required to draw multiple points, when there are Point messages being sent on multiple topics.

We don't make any assumptions about what a visualization needs to do (user-defined visualizations just run arbitrary code within the Unity scene, so they can really do whatever the user wants - spawn prefabs, move objects around, play audio... whatever works), but we do provide some useful components.

# HudPanel and HudWindows

https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/laurie/HudRefactor/com.unity.robotics.ros-tcp-connector/Runtime/TcpConnector/HUDPanel.cs
- For visualizing 2d or text based data, we offer a simple GUI system. A visualizer can open a window to show whatever information is relevant.
- The hud will attempt to automatically lay out these windows for you; or the user can manually drag and resize them. The layout can be saved and exported to a file.
- The hud is also designed to be user customizable; Users can also add their own tabs to the tab bar, or add new headers to the header area.
<img src="images~/HudAreas.png">

# BasicDrawing
https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/laurie/HudRefactor/com.unity.robotics.message-visualizations/Runtime/BasicDrawing/Scripts/BasicDrawing.cs
- BasicDrawing (name might need to change) is an easy to use, high performance 3d drawing component. It works well with visualizations but is perfectly functional on its own; users can use it for whatever purpose they want.
- It offers functions for drawing 3d points, lines, text labels in 3d space, simple shapes (cube, sphere, cone), textured squares, arbitrary meshes and point clouds.
- Each point in a point cloud is a billboarded quad. The billboarding is done in a shader on the GPU side, so the performance is extremely good; in tests on my laptop I was able to move around and view a static point cloud of 10 million points at an acceptable framerate. (Moving points would be a different story).

# Static drawing functions

- Separate from the components I've described so far, here is another important piece of the puzzle: As far as possible, each of our DefaultVisualizer classes does its job in a static function that can be called by other visualizations.
- The goal is to provide a library of useful visualization functions for users to build into their own visualizations.
- Our visualizers also reuse each other this way. Here's a good example - https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/3772e3ad460a61d0e3ffd097ba10fa9e5f147e55/com.unity.robotics.message-visualizations/Runtime/DefaultVisualizers/Geometry/TwistWithCovarianceDefaultVisualizer.cs#L24 this static function simply calls out to another visualizer to display the twist message. (In future we may add a visualization of the covariance here.)
