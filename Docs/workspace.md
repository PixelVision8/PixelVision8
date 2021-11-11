The `Workspace` is a special sandbox that is automatically created the first time Pixel Vision 8 boots up. It is accessible as part of the Virtual File System allowing you to read and write to it without fear of accidentally accessing other ares of the user's computer. 

Here are the paths to the Workspace Folder based on the OS you are running Pixel Vision 8.

| Version | Path                                           |
| ------- | ---------------------------------------------- |
| Windows | C:\\Users\\UserName\\Documents\\PixelVision8\\ |
| MacOS   | /Users/UserName/PixelVision8/PixelVision8/     |
| Linux   | /Users/UserName/PixelVision8/PixelVision8/     |

> If you are using Pixel Vision OS, it will create a `Workspace` folder inside of the root of the `PixelVision8` directory and that will be mapped to `/Workspace/` in the virtual file system. You'll still be able to access the `PixelVision8` folder at `/User/` in the virtual filesystem.

One of the advantages of the `PixelVision8` directory is a place to store the games you are working on to make it easier to use external text and image editors when making PV8 games. You can drag any game folder onto PV8 in order to load it and changes you make can be reloaded while running a game or tool by pressing `Ctrl + 4`. Feel free to use whatever tools you are comfortable with when creating PV8 games. The workflow was designed to be open so you can be as productive as possible.

In addition to the `Workspace`, Pixel Vision 8 also creates a folder inside of the user's local storage. This can be accessed at `/Shared/` inside of PV8 Virtual File System. This location is different based on the OS you are running Pixel Vision 8 on.

| Platform | Path                                           |
| -------- | ---------------------------------------------- |
| Windows  | C:\Users\UserName\AppData\Local\PixelVision8\  |
| MacOS    | /Users/UserName/.local/share/PixelVision8/     |
| Linux    | /Users/UserName/.local/share/PixelVision8/Tmp/ |

A `Tmp` folder is automatically created here when Pixel Vision 8 boots up and is removed when it is closed. This is where temporary files are stored and can be accessed at `/Tmp/` in PV8's Virtual File System.
