TCellular = {}

-- TCellular CONSTRUCTOR ------------------------------------------------------

function TCellular:New() 

    local tCell = { 
            _w = 128,
			_h = 128,
			_wallSpawnChance = 0.6,
	    	_birthLimit = 3,
	    	_deathLimit = 5,
			time = 0,
			delay = 100,
	    	_data = {},
			canvas = NewCanvas(128, 128)
        } 

    setmetatable(tCell, self) 
    self.__index = self 

    return tCell 

end  

-- TCellular Methods ------------------------------------------------------

-- conver 2d [x,y] map to 1d array
function TCellular:Pos(x,y)
	return ((y-1)*self._w + (x-1))
end

-- generate empty map
function TCellular:GenerateEmptyMap()
	for x=1,self._w do
		for y=1,self._h do
			self._data[self:Pos(x,y)]=0
		end
	end
end

-- random fill map
function TCellular:RandomFillMap()	
	for x=1,self._w do
		for y=1,self._h do
			self._data[self:Pos(x,y)]=math.floor(math.random(0,2))
		end
	end
end

-- calculate Neighbors count
function TCellular:CountLivingNeighbors(x,y)
	local count = 0

	for i=-1,1 do
		for j=-1,1 do
			local nb_x = i+x
			local nb_y = j+y
			if (nb_x < 1 or nb_y < 1 or nb_x >= self._w or nb_y >= self._h) then count=count+1 end
            if (self._data[CalculateIndex(nb_x, nb_y, self._w) -1] == 0) then  count=count+1 end
		end
	end
	return count
end

-- apply cellular rules to map
function TCellular:StepSimulation()

	local newMap = {}
	for cx=1,self._w do
		for cy=1,self._h do

			local livingNeighbors = self:CountLivingNeighbors(cx, cy)
			
			if (self._data[self:Pos(cx, cy)] == 1) then
                
                    if (livingNeighbors < self._deathLimit) then
                        newMap[self:Pos(cx, cy)] = 1
                    else
                        newMap[self:Pos(cx, cy)] = 0
                    end
                
                else
                
                    if (livingNeighbors > self._birthLimit) then
                        newMap[self:Pos(cx, cy)] = 0
                    else
                        newMap[self:Pos(cx, cy)] = 1
                    end
                end
            
		end
	end
	self._data = newMap
end

-- generate cellular automata
-- w = width
-- h = height
-- wallChange = probability for wall
-- birthLimit = count for birth cell (wall)
-- deathLimit = count for empty cell
-- smoothCount = count of stepSimulation loop
function TCellular:Generate(w,h,wallChance,birthLimit,deathLimit,smoothCount)
	
	-- print("Start")
	self._w = w
	self._h = h
	self._wallSpawnChance = wallChance
	self._birthLimit = birthLimit;
    self._deathLimit = deathLimit;
    self._data = {}

	--Generate random wall cells
    self:RandomFillMap();
	
	self.currentStep = 0
	self.totalSteps = smoothCount
	self.time = self.delay

	-- Simulate Step => smooth
	-- for i = 1,smoothCount do
	-- 	self:StepSimulation()
    -- end
	-- print("Done")
end

-- get map value at X,Y
function TCellular:Map(x,y)
	return self._data[self:Pos(x,y)]
end

function TCellular:PreviewMap(ox,oy)
	print(cave._w, cave._h)

	local total = cave._w * cave._h

	for i = 1, total do
		local pos = CalculatePosition(i-1, cave._w)
	end

	for xm=1,cave._w do
		for ym=1,cave._h do
			DrawPixels({cave:Map(xm,ym)+12},ox+xm,oy+ym,1,1);
		end
	end
end

function TCellular:GetMapData()
	return self._data;
end

function TCellular:Update(timeDelta)



	if(self.currentStep < self.totalSteps) then
		
		self.time = self.time + timeDelta

		if(self.time > self.delay) then
			self.time = 0

			self:StepSimulation()

			self.currentStep = self.currentStep + 1

			-- Copy data into canvas
			
		end
	end


end