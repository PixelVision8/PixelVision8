-- print("JSON Lang")

local function token(stream, state)
  if state.tokenizer == "base" then
    local char = stream:next() --Read a character from the stream
    
    if char == '"' or char == "'" then
      state.starter = char
      state.tokenizer = "string"
      return "string" -- Return immediatelly so quotes doesn't get affected by escape sequences

      -- Decimal numbers
    elseif char == '.' and stream:match('%d+') then
      return "number"

      -- Hex
    elseif char == "0" and stream:eat("[xX]") then
      stream:eatChain("%x")
      return "number"

      -- Ints and floats numbers
    elseif char:find('%d') then
      stream:eatChain("%d")
      stream:match("\\.%d+")
      local nextChar = stream:peek() or "" -- TODO: Do this to hex and decimals too
      if not nextChar:find("[%w_]") then
        return "number"
      end

    end
  end

  if state.tokenizer == "string" then
    local char = stream:next()
   
    if char == state.starter then
      state.starter = ""
      state.tokenizer = "base"
    else
      if stream:eol() then state.tokenizer = "base" end
    end

    return "string"

  end
end

_G["lua-tokenizer"] = {
  startState = {
    tokenizer = "base",
    multilen = 0, --Multiline comment/string '==' number, example: [=[ COMMENT ]=] -> 1
    starter = ""
  },
  token = token
}
