--LIKO-12's SyntaxHighligher API, by lhsazevedo
highlighter = {}

--The parser is for parsing the syntax and identifying which is text, which is comment, which is number, etc...
LoadScript("code-syntax-parser")

--The colorizer is for applying a colors theme on parser output, so it could be used for printing.
LoadScript("code-colorizer")

--Set the highlighter theme, example:
--[[
local highlighterTheme = {
  text = 7,
  keyword = 10,
  number = 12,
  comment = 13,
  string = 11,
  api = 14,
  callback = 15,
  selection = 6,
  escape = 12
}
]]
function highlighter:setTheme(theme)
  colorizer:setTheme(theme)
end

--Set the highlighter syntax/lanuage, example: lua
function highlighter:setSyntax(syntax)

  if(self.currentSyntax ~= syntax) then
    self.currentSyntax = syntax
    parser:loadParser(syntax)
  end

end

--Highlight a single line, lineIndex is the number of the line inside of a lines buffer, made for use in Code editors.
function highlighter:highlightLine(line, lineIndex)
  local lines, colateral = self:highlightLines({line}, lineIndex)

  return lines[1], colateral
end

--Highlight a table of lines, lineIndex is the number of the line inside of a lines buffer, made for use in Code editors.
function highlighter:highlightLines(lines, lineIndex)
  local highlightedLines = {}
  local parsedLines, colateral = parser:parseLines(lines, lineIndex)
  for _, line in ipairs(parsedLines) do
    table.insert(highlightedLines, colorizer:colorizeLine(line))
  end
  return highlightedLines, colateral
end

-- return highlighter