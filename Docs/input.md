Pixel Vision 8 has three main types of input: Controller, Keyboard, and Mouse. Each of these input types maps to specific APIs which help simplify getting the current input state of a given button easier. 

## Input States

Here are the two options in the `InputState `enum:

| Enum                 | Int |
|----------------------|-----|
| InputState\.Down     | 0   |
| InputState\.Released | 1   |

When calling the `Button()` method and supplying `InputState`.`Released`, it returns true if the button was down in the previous frame and is up in the current frame. Just like with the Buttons enum, you can use the `InputState `enum or supply an int for either state.

## Controller Input

Let's take a look at Pixel Vision 8's primary source of input which is the controller. There are two controllers for player one and two. The first controller ID is 0 and the second is 1. You can get the state of a given controller by calling the `Button()` method:

`bool Button( button, state, controllerID)`

The `Button()` method returns a boolean and needs to supply a Button ID, an input state, and the controller id. By default, the input state is set to `Buttons.Down`, and the controller is the first player. There is an enum for each of the controller's buttons:

| Enum            | Int |
|-----------------|-----|
| Buttons\.Up     | 0   |
| Buttons\.Down   | 1   |
| Buttons\.Left   | 2   |
| Buttons\.Right  | 3   |
| Buttons\.A      | 4   |
| Buttons\.B      | 5   |
| Buttons\.Select | 6   |
| Buttons\.Start  | 7   |

For example, if you just want to find the state of the A button on player one's controller you can use the enum, `Button.A`, or the value, `0`, when calling the `Button()` method:

```lua
value = Button(Buttons.A)
```

This returns true if the button is down during the current frame. Sometimes you want to know if the button was released. You can supply a different input state when calling the `Button()` method. 

Finally, as mentioned earlier, by default calling the `Button()` method returns the input state for the first player. If you want to find out the second player's input, just pass in `1` for the last argument like so:

```lua
value = Button( Buttons.A, InputeState.Released, 1)
```

It is important to note that both controllers are always "connected." You can get their value at any time in your game's code, and you do not need to worry about them being connected or disconnected from the system. If for some reason there is no controller, it will return false.

## Mouse Input

Pixel Vision 8 offers access to the mouse’s position and button states. Unlike controllers or keys, this is considered a special input and displaying a cursor or pointer will have to be manually drawn to the screen.

To access the mouse’s position, you can call the `MousePosition()` method:

```lua
value = MousePosition()
```

This will return a `Point `containing the current `X` and `Y` value for the mouse. The mouse position can not be manually changed. It’s directly tied to the native system’s mouse pointer.

In addition to getting the mouse position, you can also poll the mouse’s button state. This can be done by calling the` MouseButton()` method. It requires a button ID and state value.

| ID | Button |
|----|--------|
| 0  | Left   |
| 1  | Right  |


When calling the `MouseButton()` method and supplying `InputState`.`Released`, it returns `true `if the mouse button was down in the previous frame and is up in the current frame. Just like with the Buttons enum, you can use the `InputState `enum or supply an int for either state. 

Finally, if you do not supply a button ID or input state, the `MouseButton()` method will automatically default to the left mouse button and the down input state.

## Keyboard Input

If Pixel Vision 8 has access to the keyboard, you can capture keypresses by calling the `Key()` method:

```lua
Key(key, state)
```

The Key() method returns a boolean and you need to supply the Keycode via the Keys enum. By default, the input state is set to `Buttons.Down`. For example, if you just want to find the state of the A key, you can use the enum, `Keys.A`, or the value, `65`, when calling the `Key()` method:

```lua
value = Key(Keys.A)
```

This returns true if the A key is down during the current frame. Sometimes you want to know if the key was released. You can supply a different input state when calling the `Key()` method. 

When calling the `Key()` method and supplying `InputState`.`Released`, it returns true if the button was down in the previous frame but is not down in the current frame. Just like with the Buttons enum, you can use the `InputState `enum or supply an int for either state.

## Input String

If you are looking to just capture the last frame’s entered characters as text, for input fields, you can call `InputString()`.  This will return a string you can use to display text that the user types. Keep in mind that characters like return, delete and escape are not included in this value.

## Keys

It’s important to remember that the keyboard should only be used for tools. You can’t assume that all Pixel Vision 8 games will have access to a keyboard. The Keys enum is used to detect keyboard button presses when calling Key(). Simply pass in the enum for the key you want to test and the input state.

| Enum            | Value | Enum                | Value |
|-----------------|-------|---------------------|-------|
| Keys\.None      | 0     | Keys\.S             | 83    |
| Keys\.Backspace | 8     | Keys\.T             | 84    |
| Keys\.Tab       | 9     | Keys\.U             | 85    |
| Keys\.Enter     | 13    | Keys\.V             | 86    |
| Keys\.Escape    | 27    | Keys\.W             | 87    |
| Keys\.Space     | 32    | Keys\.X             | 88    |
| Keys\.PageUp    | 33    | Keys\.Y             | 89    |
| Keys\.PageDown  | 34    | Keys\.Z             | 90    |
| Keys\.End       | 35    | Keys\.NumPad0       | 96    |
| Keys\.Home      | 36    | Keys\.NumPad1       | 97    |
| Keys\.Left      | 37    | Keys\.NumPad2       | 98    |
| Keys\.Up        | 38    | Keys\.NumPad3       | 99    |
| Keys\.Right     | 39    | Keys\.NumPad4       | 100   |
| Keys\.Down      | 40    | Keys\.NumPad5       | 101   |
| Keys\.Insert    | 45    | Keys\.NumPad6       | 102   |
| Keys\.Delete    | 46    | Keys\.NumPad7       | 103   |
| Keys\.Alpha0    | 48    | Keys\.NumPad8       | 104   |
| Keys\.Alpha1    | 49    | Keys\.NumPad9       | 105   |
| Keys\.Alpha2    | 50    | Keys\.Multiply      | 106   |
| Keys\.Alpha3    | 51    | Keys\.Add           | 107   |
| Keys\.Alpha4    | 52    | Keys\.Separator     | 108   |
| Keys\.Alpha5    | 53    | Keys\.Subtract      | 109   |
| Keys\.Alpha6    | 54    | Keys\.Decimal       | 110   |
| Keys\.Alpha7    | 55    | Keys\.Divide        | 111   |
| Keys\.Alpha8    | 56    | Keys\.LeftShift     | 160   |
| Keys\.Alpha9    | 57    | Keys\.RightShift    | 161   |
| Keys\.A         | 65    | Keys\.LeftControl   | 162   |
| Keys\.B         | 66    | Keys\.RightControl  | 163   |
| Keys\.C         | 67    | Keys\.LeftAlt       | 164   |
| Keys\.D         | 68    | Keys\.RightAlt      | 165   |
| Keys\.E         | 69    | Keys\.Semicolon     | 186   |
| Keys\.F         | 70    | Keys\.Plus          | 187   |
| Keys\.G         | 71    | Keys\.Comma         | 188   |
| Keys\.H         | 72    | Keys\.Minus         | 189   |
| Keys\.I         | 73    | Keys\.Period        | 190   |
| Keys\.J         | 74    | Keys\.Question      | 191   |
| Keys\.K         | 75    | Keys\.Tilde         | 192   |
| Keys\.L         | 76    | Keys\.OpenBrackets  | 219   |
| Keys\.M         | 77    | Keys\.Pipe          | 220   |
| Keys\.N         | 78    | Keys\.CloseBrackets | 221   |
| Keys\.O         | 79    | Keys\.Quotes        | 222   |
| Keys\.P         | 80    | Keys\.Backslash     | 226   |
| Keys\.Q         | 81    | Keys\.OemClear      | 254   |
| Keys\.R         | 82    |                     |       |