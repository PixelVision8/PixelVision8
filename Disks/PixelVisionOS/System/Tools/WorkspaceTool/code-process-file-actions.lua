local files = {}
local totalFiles = 0
local nextFile = 0
local destPath = nil
local action = nil
local fileCleanup = {}
local fileActionPathFilter = nil

function CalculateSteps()

    -- print("CalculateSteps", dump(_G["args"]))

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
    fileActionPathFilter = _G["args"][1] == "" and nil or _G["args"][1]
    table.remove(_G["args"], 1)

    -- Get files
    files = _G["args"]

    print("total files", #files)
    -- Make sure there are files to work with
    if(#files < 1) then
        return
    end

    -- Loop through the rest of the arguments to get the files
    for i = 1, #_G["args"] do

        -- Convert the string to a path
        local path = NewWorkspacePath(_G["args"][i])

        -- Make sure that the file exists
        if(PathExists(path)) then
            
            -- Add the file to the list
            table.insert(files, path)

            -- Add a new file action step
            -- AddStep("SingleFileAction")
            AddStep("OnFileActionNextStep")

        end
        
    end

    -- Get the total files
    totalFiles = #files

    -- After all of the above steps are complete we'll clean up everything
    AddStep("CleanUpFiles")


end

function OnFileActionNextStep()

    -- Increment the counter
    nextFile = nextFile + 1


    -- -- Test to see if the counter is equil to the total
    -- if(nextFile > fileActionActiveTotal) then

    --     fileActionDelay = 4
    --     return
    -- end

    local filePath = NewWorkspacePath(files[nextFile])

    print("!File Action", nextFile, filePath)


    -- -- -- Look to see if the modal exists
    -- if(progressModal == nil) then
    --     --
    --     --   -- Create the model
    --     progressModal = ProgressModal:Init("File Action ", editorUI)

    --     -- Open the modal
    --     pixelVisionOS:OpenModal(progressModal)

    -- end

    -- local message = action .. " "..string.lpad(tostring(nextFile), string.len(tostring(fileActionActiveTotal)), "0") .. " of " .. fileActionActiveTotal .. ".\n\n\nDo not restart or shut down Pixel Vision 8."

    -- local percent = (nextFile / fileActionActiveTotal)

    -- progressModal:UpdateMessage(message, percent)

    print("TEST", action, NewWorkspacePath(destPath.Path .. filePath.Path:sub( #srcPath.Path + 1)))

    local finalDestPath = action == "delete" and destPath or NewWorkspacePath(destPath.Path .. filePath.Path:sub( #srcPath.Path + 1))
    -- local finalDestPath = action == "delete" and destPath or NewWorkspacePath(destPath.Path .. filePath.Path:sub( #srcPath.Path + 1))

    -- print("Final Dest", finalDestPath)
    if(fileActionPathFilter ~= nil) then

        finalDestPath = NewWorkspacePath(fileActionPathFilter .. finalDestPath.Path:sub( #filePath.Path + 1))

    end

    -- -- Find the path to the directory being copied
    local dirPath = finalDestPath.IsFile and finalDestPath.ParentPath or finalDestPath

    -- Make sure the directory exists
    if(PathExists(dirPath) == false) then
        CreateDirectory(dirPath)
    end

    -- print("Preparin to", action, "file", nextFile)
    if(filePath.IsFile) then
        TriggerSingleFileAction(filePath, finalDestPath, action)
    elseif(action ~= "copy") then
        table.insert(fileCleanup, filePath)
    end

end

function TriggerSingleFileAction(filePath, destPath, action)

    print("Trigger File Action", filePath, destPath, action)

    -- Copy the file to the new location, if a file with the same name exists it will be overwritten
    if(action == "copy") then

        -- Only copy files over since we create the directory in the previous step
        if(destPath.isFile) then
            -- print("CopyTo", filePath, destPath)
            CopyTo(filePath, destPath)
        end

    elseif(action == "move" or action == "throw out") then

        -- Need to keep track of directories that listed since we want to clean them up when they are empty at the end
        if(filePath.IsDirectory) then
        --     -- print("Save file path", filePath)
            table.insert(fileCleanup, filePath)
        else
            MoveTo(filePath, destPath)
            print("MoveTo", filePath, destPath)
        end

    elseif(action == "delete") then
        if(filePath.IsDirectory) then
            -- print("Save file path", filePath)
            table.insert(fileCleanup, filePath)
        else
            Delete(filePath)
            -- print("MoveTo", filePath, destPath)
        end
    else
        -- nothing happened so exit before we refresh the window
        return
    end

    -- Refresh the window
    -- RefreshWindow()

end

-- function SingleFileAction()

--     local filePath = files[nextFile]

--     print("TriggerSingleFileAction", filePath, finalDestPath, action)


--     if(PathExists(filePath) == false) then
--         return
--     end

    
--     -- Find the path to the directory being copied
--     local dirPath = destPath.IsFile and destPath.ParentPath or destPath

--     -- Make sure the directory exists
--     if(PathExists(dirPath) == false) then

--         CreateDirectory(dirPath)

--     end

--     if(filePath.IsFile) then

--         -- Copy the file to the new location, if a file with the same name exists it will be overwritten
--         if(action == "copy") then

--             -- Only copy files over since we create the directory in the previous step
--             if(destPath.isFile) then
--                 CopyTo(filePath, destPath)
--                 -- print(action, filePath, destPath)

--             end

--         elseif(action == "move" or action == "throw out") then

--             print(action, filePath, destPath)

--             -- Need to keep track of directories that listed since we want to clean them up when they are empty at the end
--             if(filePath.IsDirectory) then
--                 table.insert(fileCleanup, filePath)
--             else
--                 MoveTo(filePath, destPath)
--             end

--         elseif(action == "delete") then

--             print(action, filePath, destPath)
            
--             if(filePath.IsDirectory) then
--                 table.insert(fileCleanup, filePath)
--             else
--                 Delete(filePath)
--             end

--         end

--     elseif(action ~= "copy") then

--         table.insert(fileCleanup, filePath)
--     end
--     -- Move to the next file ID
--     nextFile = nextFile + 1

-- end

function CleanUpFiles()

    if(fileCleanup ~= nil) then

        -- TODO perform any cleanup after moving
        for i = 1, #fileCleanup do
            local path = fileCleanup[i]
            print("Cleanup", path)
            if(PathExists(path)) then
                Delete(path)
            end

        end

    end

end
