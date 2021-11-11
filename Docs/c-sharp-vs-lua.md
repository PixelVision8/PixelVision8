The API documentation was designed to be used for creating C# or Lua games. If you are not familiar with one of the languages, this document contains some helpful hints when reading the documentation. 

## Data Structures

For the most part, C# and Lua share the same data structures such as string, int, and bool. Where C# and Lua differ are arrays and objects. Technically in Lua, there are only tables. These can act like objects or arrays. In C# there is no generic object type similar to Lua’s tables. The closest data structure would be a Dictionary.

In the documentation, you may see some of the following types. This chart will help you understand how Pixel Vision 8’s Lua interpreter converts them under the hood.

| Type       | C\#               | Lua                          | Description                                                                                                                                                                   |
|------------|-------------------|------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| int\[\]    | Array of integers | Array \(table\) of integers  | This is commonly used when working with pixel data\. PV8 uses 1D integer arrays to represent sprite pixel data\.                                                              |
| string\[\] | Array of strings  | Array \(table\) of strings   | This is commonly used in text manipulation\.                                                                                                                                  |
| Dictionary | Dictionary        | Table                        | Both data structures use strings for keys\. In C\# the value will be of a fixed type whereas Lua allows for mixing and matching types for each key’s value\.                  |
| Point      | Point             | Point                        | While you can use the C\# Point class in Lua, you will need to call CreatePoint\(\) to get a new instance\. The same API works in C\# but you can use new Point\(\) as well\. |
| Rect       | Rectangle         | Rect                         | Similar to the Point class in Lua, you will need to call CreateRect\(\) to get a new instance\. The same API works in C\# but you can use new Rectangle\(\) as well\.         |

## Numbers

Both C# and Lua support multiple number types. Pixel Vision 8 on the other hand, uses integers internally. That means that in C# you’ll need to pass in integers to any API that accepts a number value. In Lua, you don’t have to convert the type, since the language doesn’t have explicit types. The one thing to keep in mind is that Pixel Vision 8’s APIs will always return integers. While you are welcome to use non-integer values in your game, you’ll want to pay attention to the removal of any fractions when a PV8 API returns a number.

The most common example of this is trying to keep track of an entity’s position. Pixel Vision 8 will for that position to "snap" to the gid since the geometry primitives, Point and Rectangle, use integers. You can track sub-pixel movement outside of Pixel Vision 8’s own data structures, just be sure to convert them to integers before applying them back to the entity’s draw call. You can use `math.floor()` in Lua or cast the number to int in C#.

## Array Index

Perhaps the biggest difference between C# and Lua are array indexes. C# is zero based and Lua is one based. That means you’ll need to manage to convert between the two by hand. Pure C# APIs that are exposed to Lua will be zero-based. When reading data structures in Lua, even ones returned by C#, you’ll use one-based indexing. This is probably one of the most common causes of bugs, especially when porting from Lua to C#.

The documentation will comment on when an API is zero-based regardless of language. API calls to colors, sprites, tiles, etc will all start at 0. For example, if you have 256 sprites, the first sprite ID will be 0 and the last will be 255. This works the same in C# and Lua. However, if you call Colors(), which returns a string array of the system’s colors, in C# you’ll access the first color at 0 and in Lua at 1.

## Null

In C#, an empty value is usually defined as null. In Lua, the equivalent is nil. Pixel Vision 8’s Lua interpreter automatically converts between the types based on which side of the engine the call is made from.

## Optional Arguments

Pixel Vision 8’s APIs may contain optional arguments. In C# these may appear with a question mark next to the type. The documentation will mention if an argument is optional or not and what default value it will use when not supplied. In cases where there are multiple optional arguments, you can use null in C# or nil in Lua to have the default value or behavior applied.

Keep in mind that some APIs have different functionality based on if an argument is provided or not. For example, calling `Sprite(0)` will return the pixel data for the first sprite. If you supply the optional value, an int array, it will update the first sprite’s pixel data and return the new pixel data int array.

## Quick Changes

Here are a few pointers to help when it comes to porting from Lua to C# or vise verse:

* You can replace `local `in Lua with var in `C#`.

* Replace Lua comments `--` with C# comments are `//`.

* You’ll need to add `;` to end all C# code lines but in Lua there is no need for that.

* Replace Lua string concatenation `..` with `+` in C#.

* Replace Lua comment blocks `--[[ ]] --` with `/** **/` in  C#.

* In most cases, you can replace `function `in Lua with `private void` in C# if the function doesn’t return a value. You’ll want to change any of the lifecycle APIs from `function Init()`  for example to `public override void Init()` in C#.

* Lua tables `=  {}` can be replaced with C# Lists `= new List<TYPE>()` in most cases. Just make sure to add the correct type.

* Replace end in  Lua with `}` in C# and be sure to go back over any function or condition code block and add `{` in C# since Lua doesn’t have this. You can also replace `then `in Lua with `{` in C#.

* Replace for `i = 1, TOTAL do`  in Lua with `for (int i = 0; i < UPPER; i++)` in C#. Remember that data structures in C# are zero-based unlike Lua where they are 1 based.

* Replace Lua `tostring()` with C#’s `.ToString()`.

* Replace Lua `not` with `!` in C#.

* You’ll need to add a few extra lines of code to replace Lua’s `math.random` in C#.  Start by creating a new instance of  `Random random = Random()` at the top of the class. Then replace `math.random` in Lua with `random.Next`.

* Replace `string.char` in Lua with `Convert.ToChar` in C#.

* Replace `math.floor` in Lua with `MathUtil.FloorToInt` in C#.

* Replace `and` in Lua with `&&` in C#

* Replace `or `in Lua with `||` in C#

* Replace `for key, value in next, VALUE` in Lua to `foreach (var keyValuePair in VALUE)` in C#

 


