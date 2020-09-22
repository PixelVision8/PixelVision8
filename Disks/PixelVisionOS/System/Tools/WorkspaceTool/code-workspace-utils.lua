-- private readonly List<string> ignoreFiles = new List<string>();

local validFiles = 
{
  ".png",
  ".json",
  ".txt",
  ".lua",
  ".pv8",
  ".pvr",
  ".wav",
  ".gif"
}

function GetDirectoryContents(workspacePath)

  -- Create empty entities table
  local entities = {}

  -- Get a list of entity
  local srcEntities = GetEntities(workspacePath)

  -- Make sure the src entity value is not empty
  if(srcEntities ~= nil) then

    -- Get the total and create a entity placeholder
    local total = #srcEntities
    local tmpEntity = nil

    -- Loop through each entity
    for i = 1, total do

      tmpEntity = srcEntities[i]

      -- TODO loop through entity and make table
      local tmpFile = {

        fullName = tmpEntity.EntityName,
        isDirectory = tmpEntity.IsDirectory,

        parentPath = tmpEntity.ParentPath.Path,
        path = tmpEntity.Path,
        selected = false,
        ext = "",
        type = "none"

      }

      -- Split the file name by .
      local nameSplit = string.split(tmpFile.fullName, ".")

      -- The file name is the first item in the array
      tmpFile.name = nameSplit[1]

      if(tmpFile.isDirectory) then

        tmpFile.type = "folder"

        -- Insert the table
        table.insert(entities, tmpFile)

      else

        tmpFile.ext = tmpEntity.GetExtension()

        if(table.indexOf(validFiles, tmpFile.ext) > - 1) then

          -- Remove the first item from the name split since it's already used as the name
          table.remove(nameSplit, 1)

          -- Join the nameSplit table with . to create the type
          tmpFile.type = table.concat(nameSplit, ".")

          table.insert(entities, tmpFile)
        end

      end

    end

  end

  return entities

end
