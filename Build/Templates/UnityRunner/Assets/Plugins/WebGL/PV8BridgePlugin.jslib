mergeInto(LibraryManager.library, {
  
  GetURL: function () {

	var returnStr = window.gameURL;
    var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
    writeStringToMemory(returnStr, buffer);
    return buffer;

  }

});