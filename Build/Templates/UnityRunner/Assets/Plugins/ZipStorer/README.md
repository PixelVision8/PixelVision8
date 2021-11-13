# ZipStorer
A Pure C# class for storing files in Zip format

[![NuGet](https://img.shields.io/nuget/v/ZipStorer.svg)](https://www.nuget.org/packages/ZipStorer/)
[![github](https://img.shields.io/github/stars/jaime-olivares/zipstorer.svg)]()

## Introduction
There are many techniques to produce Zip files in a .NET environment, like the following:

* Using the *java.util.zip* namespace
* Invoking Shell API features
* Using a third-party .NET library
* Wrapping and marshalling a non-.NET library
* Invoking a compression tool at command-line

ZipStorer is a minimalistic class to create Zip files and store/retrieve files to/from it, by using the Deflate algorithm. No other compression methods supported.

Notice that .NET 3.0 and up Frameworks come with the *ZipPackage* class, but it is not available for .NET 2.0 or Compact Framework applications. A restriction of *ZipPackage* is that you cannot avoid generating an extra file inside named *[Content_Type].xml*.

## Using the code
The ZipStorer class is the unique one needed to create the zip file. It contains a nested structure *(ZipFileEntry)* for collecting each directory entry. The class has been declared inside the System.IO namespace. 

There is no default constructor. There are two ways to construct a new ZipStorer instance, depending on specific needs: use either the *Create()* or the *Open()* static method. To create a new Zip file, use the *Create()* method like this:

````csharp
ZipStorer zip = ZipStorer.Create(filename, comment);  // file-oriented version
ZipStorer zip = ZipStorer.Create(stream, comment);  // stream-oriented version
````

It is required to specify the full path for the new zip file, or pass a valid stream, and optionally add a comment. For opening an existing zip file for appending, the *Open()* method is required, like the following:

````csharp
ZipStorer zip = ZipStorer.Open(filename, fileaccess);  // file-oriented version
ZipStorer zip = ZipStorer.Open(stream, fileaccess);  // stream-oriented version
````

Where *fileaccess* should be of type *System.IO.FileAccess* enumeration type. Also, as now ZipStorer is derived from *IDisposable* interface, the using keyword can be used to ensure proper disposing of the storage resource:

````csharp
using (ZipStorer zip = ZipStorer.Create(filename, comment))
{
    // some operations with zip object
    //
}   // automatic close operation here
````

For adding files into an opened zip storage, there are two available methods:

````csharp
public void AddFile(ZipStorer.Compress _method, string _pathname, string _filenameInZip, string _comment);
public void AddStream(ZipStorer.Compress _method, string _filenameInZip, Stream _source, DateTime _modTime, string _comment);
````
    
The first method allows adding an existing file to the storage. The first argument receives the compression method; it can be *Store* or *Deflate* enum values. The second argument admits the physical path name, the third one allows to change the path or file name to be stored in the Zip, and the last argument inserts a comment in the storage. Notice that the folder path in the *_pathname* argument is not saved in the Zip file. Use the *_filenameInZip* argument instead to specify the folder path and filename. It can be expressed with both slashes or backslashes.

The second method allows adding data from any kind of stream object derived from the *System.IO.Stream class*. Internally, the first method opens a *FileStream* and calls the second method.

Finally, it is required to close the storage with the *Close()* method. This will save the central directory information too. Alternatively, the *Dispose()* method can be used.

## Sample application
The provided sample application is a Winforms project. It will ask for files and store the path names in a *ListBox*, along with the operation type: creating or appending, and compression method. 

## Extracting stored files
For extracting a file, the zip directory shall be read first, by using the *ReadCentralDir()* method, and then the *ExtractFile()* method, like in the following minimal sample code:

````csharp
// Open an existing zip file for reading
ZipStorer zip = ZipStorer.Open(@"c:\data\sample.zip", FileAccesss.Read);

// Read the central directory collection
List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

// Look for the desired file
foreach (ZipStorer.ZipFileEntry entry in dir)
{
    if (Path.GetFileName(entry.FilenameInZip) == "sample.jpg")
    {
        // File found, extract it
        zip.ExtractFile(entry, @"c:\data\sample.jpg");
        break;
    }
}
zip.Close();
````

## Removal of entries
Removal of entries in a zip file is a resource-consuming task. The simplest way is to copy all non-removed files into a new zip storage. The *RemoveEntries()* static method will do this exactly and will construct the ZipStorer object again. For the sake of efficiency, *RemoveEntries()* will accept many entry references in a single call, as in the following example:

````csharp
List<ZipStorer.ZipFileEntry> removeList = new List<ZipStorer.ZipFileEntry>();

foreach (object sel in listBox4.SelectedItems)
{
    removeList.Add((ZipStorer.ZipFileEntry)sel);
}

ZipStorer.RemoveEntries(ref zip, removeList);
````

## File and stream usage
The current release of ZipStorer supports both files and streams for creating and opening a zip storage. Several methods are overloaded for this dual support. The advantage of file-oriented methods is simplicity, since those methods will open or create files internally. On the other hand, stream-oriented methods are more flexible by allowing to manage zip storages in streams different than files. File-oriented methods will invoke internally to equivalent stream-oriented methods. Notice that not all streams will apply, because the library requires the streams to be randomly accessed (CanSeek = true). The RemoveEntries method will work only if the zip storage is a file.

````csharp
    // File-oriented methods:
    public static ZipStorer Create(string _filename, string _comment)
    public static ZipStorer Open(string _filename, FileAccess _access)
    public void AddFile(Compression _method, string _pathname, string _filenameInZip, string _comment)
    public bool ExtractFile(ZipFileEntry _zfe, string _filename)
    public static bool RemoveEntries(ref ZipStorer _zip, List<zipfileentry> _zfes)  // No stream-oriented equivalent

    // Stream-oriented methods:
    public static ZipStorer Create(Stream _stream, string _comment)
    public static ZipStorer Open(Stream _stream, FileAccess _access)
    public void AddStream(Compression _method, string _filenameInZip, Stream _source, DateTime _modTime, string _comment)
    public bool ExtractFile(ZipFileEntry _zfe, Stream _stream)</zipfileentry>
````
        
## Filename encoding
Traditionally, the ZIP format supported DOS encoding system (a.k.a. IBM Code Page 437) for filenames in header records, which is a serious limitation for using non-occidental and even some occidental characters. Since 2007, the ZIP format specification was improved to support Unicode's UTF-8 encoding system.

ZipStorer class detects UTF-8 encoding by reading the proper flag in each file's header information. For enforcing filenames to be encoded with UTF-8 system, set the *EncodeUTF8* member of ZipStorer class to true. All new filenames added will be encoded with UTF8. Notice this doesn't affect stored file contents at all. Also be aware that Windows Explorer's embedded Zip format facility does not recognize well the UTF-8 encoding system, as WinZip or WinRAR do.

## .Net Standard support
Now ZipStorer support .Net Standard 1.6 and hence a broad range of platforms. 

If developing with Visual Studio Code, the `project.json` file must indicate:
````json
    "dependencies": {
        "ZipStorer": "*"
    }
````

## Advantages and usage
ZipStorer has the following advantages:

* It is a short and monolithic C# class that can be embedded as source code in any project (1 source file of 33K, 700+ lines)
* No external libraries, no extra DLLs in application deployments
* No Interop calls, increments portability to Mono and other non-Windows platforms
* Can also be implemented with Mono, .NET Compact Framework and .Net Standard
* Fast storing and extracting, because the code is simple and short
* UTF8 Encoding support and ePUB compatibility
 
For implementing this class into your own project, just add the *ZipStorer.cs* class file and start using it without any restriction (see [LICENSE.md](License)).

ZipStorer is also available as a [nuget package](https://www.nuget.org/packages/ZipStorer/)
