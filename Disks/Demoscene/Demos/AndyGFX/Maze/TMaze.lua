-- Define TMaze class ---------------------------------------------

TMaze = {}

-- TMaze CONSTRUCTOR ------------------------------------------------------

function TMaze:New(w,h) 

    o = { 
    		width = w,
    		height = h,
            maze={}
        } 

    setmetatable(o, self) 
    self.__index = self 

    return o 

end   

function TMaze:Blank()

	for i=1,self.height,1 do
	 	self.maze[i]={}
	 	for j=1,self.width,1 do
	 		self.maze[i][j]={}
	 	 	self.maze[i][j].left = 0
	 	 	self.maze[i][j].right = 0
	 	 	self.maze[i][j].up = 0
	 	 	self.maze[i][j].down = 0
	 	 	self.maze[i][j].visited=0
	    end
	end
end

function TMaze:Generate()
	self:Blank()
	for i=1,self.height do
	 	for j=1,self.width do
		  	if ((i==self.height) and (j==self.width)) then
		  	-- do nothing
		  	elseif (i==self.height) then
		   	self.maze[i][j].right = 1
		   	self.maze[i][j+1].left = 1
		  	elseif (j==self.width) then
		   	self.maze[i][j].down = 1
		   	self.maze[i+1][j].up = 1
		  	elseif (math.floor(math.random(0,2))==0) then
		   	self.maze[i][j].right = 1
		   	self.maze[i][j+1].left = 1
		  	else
		   	self.maze[i][j].down = 1
		   	self.maze[i+1][j].up = 1
		  	end
	 	end
	end
end

function TMaze:Preview(ox,oy, color)
	color = color or 15
 
	for i=0,#self.maze*2,1 do DrawPixels({color},ox+0,oy+i,1,1);end
	for j=1,#self.maze[1]*2,1 do DrawPixels({color},ox+j,oy+0,1,1); end

	for i=1,#self.maze do
		for j=1,#self.maze[1] do
			DrawPixels({color},ox+j*2,oy+i*2,1,1);
			if self.maze[i][j].right==0 then 	  
				DrawPixels({color},ox+j*2,oy+i*2-1,1,1);
			end
			if self.maze[i][j].down==0 then
				DrawPixels({color},ox+j*2-1,oy+i*2,1,1);
			end
		end
	end
end