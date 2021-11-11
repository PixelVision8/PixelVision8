A `Point `is a Pixel Vision 8 primitive used for defining an `X` and `Y` position on the display. The NewPoint() API creates a new instance for you. It’s important to keep in mind that point only stores integers for the `X` and `Y` value. 

## Usage

```csharp
NewPoint ( x, y )
```

## Arguments

| Name | Value | Description                                     |
| ---- | ----- | ----------------------------------------------- |
| X    | int   | The X position of the Point as an integer\.     |
| Y    | int   | The Y position of the Point as an integer\.     |
| C    | int   | The Column position of the Point as an integer. |
| R    | int   | The Row position of the Point as an integer.    |

## Returns

| Value | Description                                |
| ----- | ------------------------------------------ |
| Point | Returns a new instance of a Point object\. |

## Example

In this example, we are going to create a new point, increase it by `1` on every frame, and display it’s `X` and `Y` values on the screen. Running this code will output the following:

![image alt text](images/NewPointOutput.png)

## Lua

```lua
-- Create a new point
local pos = NewPoint()

function Init()
  
  -- Example Title
  DrawText("NewPoint()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

end

function Update(timeDelta)

  -- Increase the position by one and have it reset back to 0 if it gets bigger than the display's boundaries
  pos.x = Repeat(pos.x + 1, Display().x)
  pos.y = Repeat(pos.y + 1, Display().y)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the X and Y value of the position
  DrawText("Position " .. pos.x .. "," .. pos.y, 8, 32, DrawMode.Sprite, "large", 15)

  -- Draw a rectangle to show the change in position
  DrawRect( pos.X, pos.Y, 8, 8, 14, DrawMode.Sprite)

end
```



## C#

```csharp
namespace PixelVision8.Player
{
    class NewPointExample : GameChip
    {
        // Store the point
        private Point pos;

        public override void Init()
        {

            // Example Title
            DrawText("NewPoint()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Use the game's NewPoint() to create a point instance
            pos = NewPoint();
        }

        public override void Update(int timeDelta)
        {

            // Increase the position by one and have it reset back to 0 if it gets bigger than the display's boundaries
            pos.X = Repeat(pos.X + 1, Display().X);
            pos.Y = Repeat(pos.Y + 1, Display().Y);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the X and Y value of the position
            DrawText("Position " + pos.X + "," + pos.Y, 8, 32, DrawMode.Sprite, "large", 15);

            // Draw a rectangle to show the change in position
            DrawRect( pos.X, pos.Y, 8, 8, 14, DrawMode.Sprite);

        }

    }
}
```





