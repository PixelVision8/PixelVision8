Pixel Vision 8 uses a special `user-bios.json` file to store any changes made to the system by the user when running. When you shutdown PV8, it automatically updates the `user-bios.json` file to save any new changes. The built-in Settings Tool will give you access to the most common properties.

If you need to manually make changes, you’ll need to open this file in an external editor from your computer’s file system. This file is not accessible from the Workspace Explorer. After opening the json file, you’ll find any user-specific properties the Pixel Vision 8 Runner references when booting up. The `user-bios.json` file is located in the following location based on your computer’s OS:

<table>
  <tr>
    <td>Version</td>
    <td>Path</td>
  </tr>
  <tr>
    <td>Windows</td>
    <td>C:\Users\UserName\AppData\Local\PixelVision8\user-bios.json</td>
  </tr>
  <tr>
    <td>MacOS</td>
    <td>/Users/UserName/.local/share/PixelVision8/user-bios.json</td>
  </tr>
  <tr>
    <td>Linux</td>
    <td>/Users/UserName/.local/share/PixelVision8/user-bios.json</td>
  </tr>
</table>



On bootup, the `bios.json` file sets the default values. After that is loaded, the `user-bios.json` file is loaded and any duplicated properties are overwritten by the user’s own preferences. That means that you may not see all of the available options inside of the `user-bios.json` file. Here is a list of each of the bios settings that you can define, their value, and a description of what they do.

<table>
  <tr>
    <td>Property</td>
    <td>Value</td>
    <td>Description</td>
  </tr>
  <tr>
    <td>AutoRun</td>
    <td>/PixelVisionOS/Tools/WorkspaceTool/</td>
    <td>Path to game or tool that will load after booting is complete.</td>
  </tr>
  <tr>
    <td>BackKey</td>
    <td>27</td>
    <td>This is mapped to the Esc key by default</td>
  </tr>
  <tr>
    <td>BaseDir</td>
    <td>PixelVision8</td>
    <td>The name of the folder used on the file system to store the Workspace and Tmp directories.</td>
  </tr>
  <tr>
    <td>BootTool</td>
    <td>/PixelVisionOS/System/Tools/BootTool/</td>
    <td>The path to the default boot tool.</td>
  </tr>
  <tr>
    <td>Brightness</td>
    <td>100</td>
    <td>A value used to calculate brightness when the CRT filter is enabled.</td>
  </tr>
  <tr>
    <td>CropScreen</td>
    <td>True</td>
    <td>A boolean flag if the screen should be cropped or show black bars.</td>
  </tr>
  <tr>
    <td>CRT</td>
    <td>False</td>
    <td>A boolean if the CRT filter should be used or not.</td>
  </tr>
  <tr>
    <td>DebugTime</td>
    <td>False</td>
    <td>A boolean that when set to true will force the time to always display as Saturday at 8:00am.</td>
  </tr>
  <tr>
    <td>Disk0</td>
    <td>none</td>
    <td>A local file system path to a .pv8 disk or folder to load on the desktop.</td>
  </tr>
  <tr>
    <td>Disk1</td>
    <td>none</td>
    <td>A local file system path to a .pv8 disk or folder to load on the desktop.</td>
  </tr>
  <tr>
    <td>ErrorTool</td>
    <td>/PixelVisionOS/System/Tools/ErrorTool/</td>
    <td>Path to the default Error Tool that is loaded when an exception is thrown by PV8.</td>
  </tr>
  <tr>
    <td>Exception</td>
    <td>@{error}\nPress Ctrl + 4 to reload the current game.</td>
    <td>Message template to display when an exception is thrown and displayed in the Error Tool.</td>
  </tr>
  <tr>
    <td>FileDiskMounting</td>
    <td>True</td>
    <td>A boolean flag that enables or disables dragging .pv8 disks onto the PV8 runner window.</td>
  </tr>
  <tr>
    <td>FullScreen</td>
    <td>False</td>
    <td>A boolean flag that forces PV8 to load in full-screen mode when starting up.</td>
  </tr>
  <tr>
    <td>LoadError</td>
    <td>There was an issue loading from '@{path}'.</td>
    <td>Message template to display when there is an issue loading a game from a specific path.</td>
  </tr>
  <tr>
    <td>LoadTool</td>
    <td>/PixelVisionOS/System/Tools/LoadTool/</td>
    <td>Path to the default Load Tool that is used to display the loading progress of a game that is about to be played.</td>
  </tr>
  <tr>
    <td>Mute</td>
    <td>False</td>
    <td>A boolean value that determines if the sound will play or be muted regardless of the volume's value.</td>
  </tr>
  <tr>
    <td>NoAutoRun</td>
    <td>Could not find an OS to boot. Insert a disk or restart in 'safe mode' by holding down the Shift key to reinstall Pixel Vision OS.</td>
    <td>Message template to display when the default autorun path can not be loaded.</td>
  </tr>
  <tr>
    <td>NoDefaultTool</td>
    <td>Could not find a default tool to load.</td>
    <td>Message template to display when no default tool path has been found when trying to exit a game.</td>
  </tr>
  <tr>
    <td>Player1AButton</td>
    <td>4</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1AKey</td>
    <td>88</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1BButton</td>
    <td>5</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1BKey</td>
    <td>67</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1DownButton</td>
    <td>1</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1DownKey</td>
    <td>40</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1LeftButton</td>
    <td>2</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1LeftKey</td>
    <td>37</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1RightButton</td>
    <td>3</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1RightKey</td>
    <td>39</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1SelectButton</td>
    <td>6</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1SelectKey</td>
    <td>65</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1StartButton</td>
    <td>7</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1StartKey</td>
    <td>83</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1UpButton</td>
    <td>0</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player1UpKey</td>
    <td>38</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2AButton</td>
    <td>4</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2AKey</td>
    <td>13</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2BButton</td>
    <td>5</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2BKey</td>
    <td>161</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2DownButton</td>
    <td>1</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2DownKey</td>
    <td>75</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2LeftButton</td>
    <td>2</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2LeftKey</td>
    <td>74</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2RightButton</td>
    <td>3</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2RightKey</td>
    <td>76</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2SelectButton</td>
    <td>6</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2SelectKey</td>
    <td>186</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2StartButton</td>
    <td>7</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2StartKey</td>
    <td>188</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2UpButton</td>
    <td>0</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>Player2UpKey</td>
    <td>73</td>
    <td>Controller key mapping</td>
  </tr>
  <tr>
    <td>RecordKey</td>
    <td>51</td>
    <td>The default key to use to toggle recording when the Ctrl key is pressed at the same time.</td>
  </tr>
  <tr>
    <td>Resolution</td>
    <td>512x480</td>
    <td>The default resolution for PV8's window. Games will be scaled up to fill this value and changing the scale value will multiply this resolution.</td>
  </tr>
  <tr>
    <td>RestartKey</td>
    <td>52</td>
    <td>The default key to use to restart the currently running game when the Ctrl key is pressed at the same time.</td>
  </tr>
  <tr>
    <td>Scale</td>
    <td>1</td>
    <td>A value between 0 and 4 that is multiplied by PV8's window resolution to make the display larger or smaller.</td>
  </tr>
  <tr>
    <td>ScreenShotKey</td>
    <td>50</td>
    <td>The default key to use for capturing screenshots when the Ctrl key is pressed at the same time.</td>
  </tr>
  <tr>
    <td>Sharpness</td>
    <td>-6</td>
    <td>This is a value between -10 and 10 that defines the sharpness value if the CRT filter is enabled. Negative numbers are sharper.</td>
  </tr>
  <tr>
    <td>StretchScreen</td>
    <td>False</td>
    <td>A boolean flag that enables or disables stretching the game's display to match the resolution of the screen instead of maintaining its own aspect ratio.</td>
  </tr>
  <tr>
    <td>SystemName</td>
    <td>Pixel Vision 8</td>
    <td>The name of the specific version of PV8. This can change between different versions and will enable or disable features in the OS.</td>
  </tr>
  <tr>
    <td>SystemVersion</td>
    <td>v0.9.6</td>
    <td>This is the current version number of the PV8 Runner.</td>
  </tr>
  <tr>
    <td>Volume</td>
    <td>40</td>
    <td>A value between 0 and 100 that represents the volume for PV8.</td>
  </tr>
  <tr>
    <td>Warning</td>
    <td>@{error}</td>
    <td>The error message template that is displayed when a system warning is thrown.</td>
  </tr>
</table>



You can call `ReadBiosData()` and `WriteBiosData()` to make changes to the bios at any time in your game. This is helpful when creating tools or if you want to store system-wide settings between sessions when you plan on creating stand-alone games.

Call `ReadBiosData()` in order to retrieve a bios string value. You can pass an optional default value that will be returned if the key is not found

`ReadBiosData(key, defaultValue)`

Call `WriteBiosData()` to save a string value to a supplied key. If the key exists in the bios, it will be overwritten by the new value

`WriteBiosData(key, value)`

It’s important to note that bios values are saved and retrieved as strings. You’ll need to do additional parsing or conversion if the value is expected to be a bool or a number.

