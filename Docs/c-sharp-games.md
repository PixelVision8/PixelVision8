By default, Pixel Vision 8 will automatically create a Lua game project. The Workspace Explorer uses the same template for all new game projects, which includes a `code.lua` file.

![image-20210609093405900](images/c-sharp-games/image-20210609093405900.png)

You can switch to C# at any time by opening the info.json file and changing the game engine to C#.

![image-20210609093618321](images/c-sharp-games/image-20210609093618321.png)

Once you switch over to the C# runner, you will need to create a new `code` file since the game will no longer run with the default `code.lua` script.

![image-20210609094006593](images/c-sharp-games/image-20210609094006593.png)

The Workspace Explorer automatically detects the runner to use and creates either a Lua or C# script file for you whenever you select New Code from the drop-down menu. You can also tell which runner you are using by the code icons. If the game's runner can not use the code file, it will change color.

![image-20210609094215016](images/c-sharp-games/image-20210609094215016.png)

You can edit C# files just like you would Lua, and you can have multiple C# files in a single project. The biggest issue to pay attention to will be giving each code file a unique class name and importing the correct code libraries.

![image-20210609094834656](images/c-sharp-games/image-20210609094834656.png)

If you run the C# game using Ctrl + R, it will load the default code.cs file along with any other C# files in the project. Since the C# code is re-compiled at run-time, there may be a pause while loading. If there are no errors, you will be able to play your C# game.

![image-20210609095013550](images/c-sharp-games/image-20210609095013550.png)

