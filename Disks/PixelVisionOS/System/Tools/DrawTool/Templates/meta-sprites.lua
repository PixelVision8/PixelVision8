-- start of sprite list

local metaSprites = {
        % s
}

-- end of sprite list

-- Call this when you are ready to create the meta sprites
function CreateMetaSprites()

    -- Change the total meta sprites
    local total = TotalMetaSprites(#metaSprites)
    
    -- Get the current sprite size
    local spriteSize = SpriteSize()
    
    -- Loop through all of the meta sprites
    for i = 1, total do
    
        -- Get a meta sprite
        local metaSpriteData = metaSprites[i]
        
        -- Calculate the real index
        local index = i-1
        
        -- Get the meta sprite at the index.
        local metaSprite = MetaSprite(index)
        
        -- Clear any sprites
        metaSprite.Clear()
        
        -- Change the meta sprite's name
        metaSprite.Name = metaSpriteData.name
        
        -- Loop through all of the sprites
        for j = 1, #metaSpriteData.spriteIDs do
            
            -- Calculate the column and row of the sprite in a grid
            local pos = CalculatePosition(j-1, metaSpriteData.width)
            
            -- Add the new sprite and offset the position by the sprite's size
            metaSprite.AddSprite(metaSpriteData.spriteIDs[j], pos.X * spriteSize.X, pos.Y * spriteSize.Y)
            
        end
        
        -- Create a global meta sprite name mapped to the meta sprite id
        _G[metaSprite.Name] = index
    
    end
end
