-- local newStream = require("Libraries.SyntaxParser.stream")

LoadScript("code-stream")

parser = {}

parser.parser = {}
parser.cache = {}
parser.state = nil

function parser:loadParser(language)
  self.cache = {}
  self.state = nil

  LoadScript("code-lang-" .. language)

  self.parser = _G["lua-tokenizer"]--require("Libraries.SyntaxParser.languages."..language)
end

function parser:previousState(lineIndex)
  lineIndex = lineIndex - 1
  while lineIndex > 0 do
    if self.cache[lineIndex] then return self.cache[lineIndex] end
    lineIndex = lineIndex - 1
  end
  return false
end

function parser:parseLines(lines, lineIndex)
  local result = {}

  -- Forget all states after the modified line
  for i, state in pairs(self.cache) do
    if i > lineIndex then
      self.cache[i] = nil
    end
  end

  -- Process lines
  local colateral = false
  for i = 1, #lines do
    local line = lines[i]
    local lineID = lineIndex + i - 1

    self.state = {}

    -- Copy previous line state table, or create a new one if needed.
    -- TODO: language should provide a copy method.
    local tempState = self.cache[lineID - 1] --Pervious line
    or self:previousState(lineIndex) --Any pervious line
    or self.parser.startState --The start state provided by the parser

    for k, v in pairs(tempState) do
      self.state[k] = v
    end

    -- Backup previous state of the current line
    local previousState = {}
    if self.cache[lineID] then
      for k, v in pairs(self.cache[lineID]) do
        previousState[k] = v
      end
    end

    -- Process line
    result[#result + 1] = parser:parseLine(line)

    -- Copy the processd state to cache.
    -- Also checks if this is the last line and its change is colateral.
    self.cache[lineID] = {}
    for k, v in pairs(self.state) do
      if i == #lines and previousState[k] ~= self.state[k] then colateral = true end
      self.cache[lineID][k] = v
    end
  end
  return result, colateral
end

function parser:parseLine(line)
  local result = {}
  local stream = newStream(line)

  while not stream:eol() do
    local token = self.parser.token(stream, self.state)
    result[#result + 1] = token or "text"
    result[#result + 1] = stream:current() --The text read by the tokenizer
    stream.start = stream.pos
  end

  if #result == 0 then return {"text", line} end
  return result
end

return parser
