A `Rectangle `is a Pixel Vision 8 primitive used for defining the bounds of an object on the display. It contains an `X`, `W`, `Width` and `Height` property. The Rectangle object class also has some additional methods to aid with collision detection.

## Usage

```csharp
NewRect ( x, y, w, h )
```

## Arguments

| Name   | Value | Description                              |
| ------ | ----- | ---------------------------------------- |
| X      | int   | The x position of the rect as an int\.   |
| Y      | int   | The y position of the rect as an int\.   |
| Width  | int   | The width value of the rect as an int\.  |
| Height | int   | The height value of the rect as an int\. |

## Returns

| Value | Description                               |
| ----- | ----------------------------------------- |
| Rect  | Returns a new instance of a Rect object\. |

## Collision APIs

The Rectangle has three methods you can use to determine if there is a collision.

| Name                    | Description                                                      |
|-------------------------|------------------------------------------------------------------|
| Contains\( x, y \)      | Test to see if an X and Y position are inside of the Rectangle\. |
| Contains\( point \)     | Test to see if a point is inside of the Rectangle\.              |
| Contains\( rectangle \) | Test to see if another Rectangle intersects with the Rectangle\. |

## Example

In this example, we are going to create two rectangles and test to see if they collide with one another. Running this code will output the following:

![image alt text](images/NewRectOutput.png)

## Lua

```lua
-- Create a rectangle
local rectA = NewRect(8, 32, 128, 128)

-- This will store the mouse position
local mousePos = NewPoint()

-- This will store the collision state
local collision = false

function Init()
  
  -- Example Title
  DrawText("NewRect()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

end

function Update(timeDelta)

  -- Get the mouse position
  mousePos = MousePosition()

  -- Test for the collision
  collision = rectA:Contains(mousePos)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw rectA and change the color if there is a collision
  DrawRect(rectA.x, rectA.y, rectA.width, rectA.height, collision and 6 or 5, DrawMode.Sprite)

  -- Draw the mouse cursor on the screen
  DrawRect(mousePos.x - 1, mousePos.y - 1, 2, 2, 15, DrawMode.Sprite)

end
```



## C#

```csharp
namespace PixelVision8.Player
{
    class NewRectExample : GameChip
    {
        // Store the rectangle
        Rectangle rectA;

        // This will store the mouse position
        private Point mousePos;

        // This will store the collision state
        private bool collision = false;

        public override void Init()
        {

            // Example Title
            DrawText("NewRect()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Use the game's NewRect() to create a rectangle instance
            rectA = NewRect(8, 32, 128, 128);

        }

        public override void Update(int timeDelta)
        {
            // Get the mouse position
            mousePos = MousePosition();

            // Test for the collision
            collision = rectA.Contains(mousePos);

        }

        public override void Draw()
        {

            // Redraw the display
            RedrawDisplay();

            // Draw rectA and change the color if there is a collision
            DrawRect(rectA.X, rectA.Y, rectA.Width, rectA.Height, collision ? 6 : 5, DrawMode.Sprite);

            // Draw the mouse cursor on the screen
            DrawRect(mousePos.X - 1, mousePos.Y - 1, 2, 2, 15, DrawMode.Sprite);

        }

    }
}
```





