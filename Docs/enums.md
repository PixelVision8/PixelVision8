Pixel Vision 8’s APIs leverage several Enums. You can find a full listing of the enums referenced below.

## Buttons

The Button enum contains all of the valid buttons on the controller.

| Enum            | Value |
|-----------------|-------|
| Buttons\.Up     | 0     |
| Buttons\.Down   | 1     |
| Buttons\.Left   | 2     |
| Buttons\.Right  | 3     |
| Buttons\.A      | 4     |
| Buttons\.B      | 5     |
| Buttons\.Select | 6     |
| Buttons\.Start  | 7     |

## InputState

The InputState enum contains the valid states of a button.

| Enum                 | Value |
|----------------------|-------|
| InputState\.Down     | 0     |
| InputState\.Released | 1     |

## DrawMode

The DrawMode enum contains all of the valid render layers and modes available to the DisplayChip.

| Enum                   | Value | Description                                                  |
| ---------------------- | ----- | ------------------------------------------------------------ |
| DrawMode\.TilemapCache | \-1   | This is a special layer that is used to draw raw pixel data on top of the tilemap\. |
| DrawMode\.Background   | 0     | This is the clear layer and is usually reserved for filling the screen with a background color\. |
| DrawMode\.SpriteBelow  | 1     | This is a layer dedicated to sprites just above the background\. |
| DrawMode\.Tile         | 2     | This is the tilemap layer and is drawn above the SpriteBelow layer allowing sprites to appear behind the background\. |
| DrawMode\.Sprite       | 3     | This is the default layer for sprites to be rendered at\. It is above the background\. |
| DrawMode\.UI           | 4     | This is a special layer which can be used to draw raw pixel data above the background and sprites\. It's designed for HUDs in your game and other graphics that do not scroll with the tilemap\. |
| DrawMode\.SpriteAbove  | 5     | This layer allows sprites to render above the UI layer\. It is useful for pop-up and drop down menus. |
| DrawMode.Mouse         | 6     | This layer is designed to render a mouse cursor above all of the other layers. |



## SaveFlags

The SaveFlags enum is used when loading or saving a game’s state. It helps define each of the pieces of data used to make a complete game when loading it into memory. This is used specifically for the GameEditor.

| Enum                | Value |
|---------------------|-------|
| SaveFlags\.None     | 0     |
| SaveFlags\.System   | 1     |
| SaveFlags\.Code     | 2     |
| SaveFlags\.Colors   | 4     |
| SaveFlags\.ColorMap | 8     |
| SaveFlags\.Sprites  | 16    |
| SaveFlags\.Tilemap  | 32    |
| SaveFlags\.Fonts    | 64    |
| SaveFlags\.Meta     | 128   |
| SaveFlags\.Music    | 256   |
| SaveFlags\.Sounds   | 512   |
| SaveFlags\.SaveData | 1024  |


## Keys

The Keys enum is used to detect keyboard button presses when calling Key(). Simply pass in the enum for the key you want to test and the input state.

| Enum            | Value | Enum                   | Value |
|-----------------|-------|------------------------|-------|
| Keys\.None      | 0     | Keys\.S                | 83    |
| Keys\.Backspace | 8     | Keys\.T                | 84    |
| Keys\.Tab       | 9     | Keys\.U                | 85    |
| Keys\.Enter     | 13    | Keys\.V                | 86    |
| Keys\.Escape    | 27    | Keys\.W                | 87    |
| Keys\.Space     | 32    | Keys\.X                | 88    |
| Keys\.PageUp    | 33    | Keys\.Y                | 89    |
| Keys\.PageDown  | 34    | Keys\.Z                | 90    |
| Keys\.End       | 35    | Keys\.NumPad0          | 96    |
| Keys\.Home      | 36    | Keys\.NumPad1          | 97    |
| Keys\.Left      | 37    | Keys\.NumPad2          | 98    |
| Keys\.Up        | 38    | Keys\.NumPad3          | 99    |
| Keys\.Right     | 39    | Keys\.NumPad4          | 100   |
| Keys\.Down      | 40    | Keys\.NumPad5          | 101   |
| Keys\.Insert    | 45    | Keys\.NumPad6          | 102   |
| Keys\.Delete    | 46    | Keys\.NumPad7          | 103   |
| Keys\.D0        | 48    | Keys\.NumPad8          | 104   |
| Keys\.D1        | 49    | Keys\.NumPad9          | 105   |
| Keys\.D2        | 50    | Keys\.Multiply         | 106   |
| Keys\.D3        | 51    | Keys\.Add              | 107   |
| Keys\.D4        | 52    | Keys\.Separator        | 108   |
| Keys\.D5        | 53    | Keys\.Subtract         | 109   |
| Keys\.D6        | 54    | Keys\.Decimal          | 110   |
| Keys\.D7        | 55    | Keys\.Divide           | 111   |
| Keys\.D8        | 56    | Keys\.LeftShift        | 160   |
| Keys\.D9        | 57    | Keys\.RightShift       | 161   |
| Keys\.A         | 65    | Keys\.LeftControl      | 162   |
| Keys\.B         | 66    | Keys\.RightControl     | 163   |
| Keys\.C         | 67    | Keys\.LeftAlt          | 164   |
| Keys\.D         | 68    | Keys\.RightAlt         | 165   |
| Keys\.E         | 69    | Keys\.OemSemicolon     | 186   |
| Keys\.F         | 70    | Keys\.OemPlus          | 187   |
| Keys\.G         | 71    | Keys\.OemComma         | 188   |
| Keys\.H         | 72    | Keys\.OemMinus         | 189   |
| Keys\.I         | 73    | Keys\.OemPeriod        | 190   |
| Keys\.J         | 74    | Keys\.OemQuestion      | 191   |
| Keys\.K         | 75    | Keys\.OemTilde         | 192   |
| Keys\.L         | 76    | Keys\.OemOpenBrackets  | 219   |
| Keys\.M         | 77    | Keys\.OemPipe          | 220   |
| Keys\.N         | 78    | Keys\.OemCloseBrackets | 221   |
| Keys\.O         | 79    | Keys\.OemQuotes        | 222   |
| Keys\.P         | 80    | Keys\.OemBackslash     | 226   |
| Keys\.Q         | 81    | Keys\.OemClear         | 254   |
| Keys\.R         | 82    |                        |       |



