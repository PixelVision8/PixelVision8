local destPath = nil
local action = nil
local filesToCopy = {}
local fileActionCounter = 1
local fileCleanup = {}


function CalculateSteps()

    -- Get the destination path
    srcPath = NewWorkspacePath(_G["args"][1])
    table.remove(_G["args"], 1)
     
    -- Get the destination path
    destPath = NewWorkspacePath(_G["args"][1])
    table.remove(_G["args"], 1)

    -- Get the action
    action = _G["args"][1]
    table.remove(_G["args"], 1)

    -- Read the file path filter and convert to nil if empty
    local duplicate = _G["args"][1] == "True"
    table.remove(_G["args"], 1)

    -- Loop through the rest of the arguments to get the files
    for i = 1, #_G["args"] do

        -- Convert the string to a path
        local filePath = NewWorkspacePath(_G["args"][i])

        -- Make sure that the file exists
        if(PathExists(filePath)) then
            
            local finalDestPath = NewWorkspacePath(destPath.Path .. filePath.Path:sub(#srcPath.Path + 1))
            
            -- print("Action", filePath.Path, finalDestPath.Path)

            if(duplicate == true and PathExists(finalDestPath)) then

                -- Make the final dest path unique
                finalDestPath = UniqueFilePath(finalDestPath)

            end

            -- Check to see if the path is a folder
            if(filePath.isDirectory == true) then

                -- Get all the children of the source folder
                local children = GetEntitiesRecursive(filePath)

                -- Loop through the children
                for i = 1, #children do
                        
                    -- Get the child path
                    local childPath = children[i]

                    local childDest = NewWorkspacePath(finalDestPath.Path .. childPath.Path:sub(#filePath.Path + 1))
                    
                    -- Create a new child destination based on the final destintaion path
                    table.insert(
                        filesToCopy, 
                        {
                            src = childPath, 
                            dest = childDest
                        }
                    )

                    -- print("Action", childPath.Path, childDest.Path)

                    -- Add a new file action to the task
                    AddStep("OnFileActionNextStep")

                end

            end

            -- Add the file to the list
            table.insert(filesToCopy, {src = filePath, dest = finalDestPath})

            -- Add a new file action step
            AddStep("OnFileActionNextStep")

        end

        -- print("tmpFileCount", #filesToCopy)

        BackgroundScriptData( "tmpFileCount", tostring(#filesToCopy) )

    end

    fileActionCounter = 1
    fileActionBasePath = destPath
    
    -- After all of the above steps are complete we'll clean up everything
    AddStep("OnFileActionComplete")


end

function OnFileActionNextStep()


    -- Get the src and dest paths from the files list
    local filePath = filesToCopy[fileActionCounter].src
    local finalDestPath = filesToCopy[fileActionCounter].dest

    -- print("OnFileActionNextStep", filePath, finalDestPath)

    
    if(filePath.IsFile) then

        TriggerSingleFileAction(filePath, finalDestPath)

    else

        -- print("Make Folder", filePath, finalDestPath.Path)

        if(action ~= "delete" and PathExists(finalDestPath) == false) then
            CreateDirectory(finalDestPath)
        end

        if(action ~= "copy") then
            table.insert(fileCleanup, filePath)
        end

    end

    fileActionCounter = fileActionCounter + 1

end

function TriggerSingleFileAction(srcPath, destPath)

    -- Check to see if the parent directory of a file exists
    if(srcPath.IsFile == true and PathExists(destPath.ParentPath) == false) then

        -- Create the directory so the file can be copied over
        CreateDirectory(destPath.ParentPath)

    end

    -- Copy the file to the new location, if a file with the same name exists it will be overwritten
    if(action == "copy") then

        -- Only copy files over since we create the directory in the previous step
        if(destPath.isFile) then
            -- print("CopyTo", srcPath, destPath)
            CopyTo(srcPath, destPath)
        end

    elseif(action == "move") then

        -- Need to keep track of directories that listed since we want to clean them up when they are empty at the end
        if(srcPath.IsDirectory) then
            -- print("Save file path", srcPath)
            table.insert(fileCleanup, srcPath)
        else
            MoveTo(srcPath, destPath)
            -- print("MoveTo", srcPath, destPath)
        end

    elseif(action == "delete") then
        if(srcPath.IsDirectory) then
            -- print("Save file path", srcPath)
            table.insert(fileCleanup, srcPath)
        else
            Delete(srcPath)
            -- print("MoveTo", srcPath, destPath)
        end
    else
        -- nothing happened so exit before we refresh the window
        return
    end

end

function OnFileActionComplete()
    -- print("File action done")
    -- TODO perform any cleanup after moving
    local totalCleanup = #fileCleanup
    for i = 1, totalCleanup do
        local path = fileCleanup[i]
        if(PathExists(path)) then
            -- print("Cleanup", path)
            Delete(path)
        end
    end

end
