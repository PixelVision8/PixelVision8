--The colorizer is for applying a colors theme on parser output, so it could be used for printing.
colorizer = {}

colorizer.defaultColor = 7
colorizer.theme = {}

function colorizer:setTheme(theme)
  self.theme = theme
end

function colorizer:getColor(name)
  return self.theme[name] or self.defaultColor
end

function colorizer:colorizeLine(line)
  local line = line
  for i, v in ipairs(line) do
    if i % 2 ~= 0 then
      line[i] = self:getColor(line[i])
    end
  end
  return line
end

function colorizer:colorizeLines(lines)
  local colorizedLines = {}
  for i, line in ipairs(lines) do
    table.insert(colorizedLines, self:colorize_line(line))
  end
  return colorizedLines
end

return colorizer
