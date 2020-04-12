newStream = function(streamData)
  local stream = {}

  stream.string = streamData or ""
  stream.start = 1
  stream.pos = stream.start

  --Read the character at the current stream position, returns nil if it reached the end of the stream.
  function stream:peek()
    local char = self.string:sub(self.pos, self.pos)
    if char ~= "" then return char else return end
  end

  --Matches a pattern at the current stream position, if the pattern matched it will push the stream position and return true.
  function stream:match(pattern)
    local s, e = self.string:find(pattern, self.pos)
    if s and s == self.pos then
      self.pos = e + 1
      return true
    end
  end

  --Eats a specific character of the stream, if the character didn't match then it will return nil, otherwise it will push the stream position and return the matched character.
  function stream:eat(pattern)
    local char = self.string:sub(self.pos, self.pos)
    if char:find(pattern) then
      self.pos = self.pos + 1
      return char
    end
  end

  --Eat characters in a row that matched a pattern, returns true if ate any, otherwise false.
  function stream:eatChain(pattern)
    local start = self.pos
    while self:eat(pattern) do end
    return self.pos > start
  end

  --Skip from the current stream position into the start of a specific character, if the character wasn't found it will return nil, otherwise true
  function stream:skipTo(char)
    local start, found = self.string:find(char, self.pos)
    if start and start >= self.pos then
      self.pos = found
      return true
    end
  end

  --Skip to the end of the stream
  function stream:skipToEnd()
    self.pos = self.string:len() + 1
  end

  --Check if the stream is at the end of it.
  function stream:eol()
    return self.pos > self.string:len()
  end

  --Read a byte/char, push the stream position by 1, and return the read char.
  function stream:next()
    if self.pos <= self.string:len() then
      self.pos = self.pos + 1
      return self.string:sub(self.pos - 1, self.pos - 1)
    end
  end

  --Go back (n or 1) characters in the steam (pulls the stream position).
  function stream:backUp(n)
    if self.pos > 1 then
      self.pos = self.pos - (n or 1)
      return true
    end
  end

  --Returns the characters from the stream start to the current stream position.
  function stream:current()
    return self.string:sub(self.start, self.pos - 1)
  end

  return stream
end
