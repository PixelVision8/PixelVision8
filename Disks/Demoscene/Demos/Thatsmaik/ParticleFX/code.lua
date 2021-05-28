--#########################################################--
--Particle System for the PV8 community
--Code by @thatsmaik
--Consider following me on twitter for more code doodles :)
--#########################################################--

--LOCAL VARIABLES--
--General
local mousePos = NewPoint(0, 0) -- Create point for saving the mouse position later on
local center = NewPoint(Display().x / 2, Display().y /2) -- Store center of the screen as point

local time = 0 -- Raw time
local msec = 100 -- Delay time
local time_msec = 0 -- Time in miliseconds

---

local canvas = NewCanvas(Display().x, Display().y)
local spriteparticles = {}
local canvasparticles = {}

--EXTRA FUNCTIONS--

function map(x, in_min, in_max, out_min, out_max)
	return out_min + (x - in_min)*(out_max - out_min)/(in_max - in_min)
end

function add_spriteparticle(_spriteid, _x, _y, _dx, _dy, _lifetime, _friction, _gravity, _fadesprites)

    local particle = {
     sp=_spriteid,
     x=_x,
     y=_y,
     dx=_dx,
     dy=_dy,
     lifetime=_lifetime,
     orig_lifetime=_lifetime,
     gravity=_gravity or 0,
     frc = _friction or 0.98,
     fade=_fadesprites or {},

     draw=function(self)
       DrawSprite(self.sp, self.x, self.y, false, false, DrawMode.Sprite)
     end,

     update=function(self)

         --apply physics
         self.x+=self.dx
         self.y+=self.dy

         self.dy+=self.gravity

         self.dx *= self.frc
         self.dy *= self.frc

         --reduce particle lifetime
         self.lifetime-=1


         --set the sprites in relation to lifetime if more then one
         if type(self.fade)=="table" then
             self.sp=self.fade[math.floor(#self.fade*(self.lifetime/self.orig_lifetime))+1]
         end

         --if the particle lifetime ended remove it from table
         if self.lifetime<0 then
             spriteparticles[self] = nil
         end
     end
   }

   spriteparticles[particle] = particle --add particle to table
end

function add_canvasparticle(_x,_y,_dx,_dy,_size,_lifetime,_friction,_gravity,_fadecolors)

    local particle = {

     x=_x,
     y=_y,
     dx=_dx,
     dy=_dy,
     lifetime=_lifetime,
     orig_lifetime=_lifetime,
     radius=_size,
     frc = _friction or 0.98,
		 gravity=_gravity or 0,
		 fade=_fadecolors,
     col=0,

     draw=function(self)
         --draw the particle
         canvas:SetStroke(self.col, 1, 1)

         canvas:DrawEllipse(self.x, self.y, self.radius, self.radius)
     end,

     update=function(self)
         --apply physics
         self.x+=self.dx
         self.y+=self.dy

         self.dy+=self.gravity

         self.dx *= self.frc
         self.dy *= self.frc

         --reduce the radius over time aka make it smaller the "older" it gets
         self.radius*=0.95

         --reduce lifetime
         self.lifetime -= 1

         --set the color in relation to lifetime
         if type(self.fade)=="table" then
             self.col=self.fade[math.floor(#self.fade*(self.lifetime/self.orig_lifetime))+1]
         else
             --or use a fixed color
             self.col=self.fade
         end

				 --if lifetime is over remove it from table
         if self.lifetime<0 then
             canvasparticles[self] = nil
         end
     end
	 }

	 canvasparticles[particle] = particle --add particle to table
end


function Init()
  -- Here we are manually changing the background color
  BackgroundColor(0)
  
  canvas:SetStroke(15, 1, 1)
end


function Update(timeDelta)

	--Calculate time
  time = time + timeDelta
	--Calculate miliseconds
	if time > msec then
		time_msec += 1
		time = 0
	end

	add_spriteparticle(0, center.x, center.y, math.cos(math.random()*(math.pi*2)), math.sin(math.random()*(math.pi*2)), math.random()*20+10, 1, 0, {3,2,1,0,1,2,3})

  add_canvasparticle(mousePos.x, mousePos.y, math.random(0,2)-1, math.random(0,2)-3, math.random(0,24)+2, 45, 1, 0.1, {7,7,7,7,7,7,6,6,6,6,6,5,5,9,9,10,10,10,10,10,8,8,8,8})

  mousePos = MousePosition() --Update mouse position

	--Update sprite particles
  for k, p in pairs(spriteparticles) do
    p:update()
  end

	--Update canvas particles
  for k, p in pairs(canvasparticles) do
    p:update()
  end
end


function Draw()

  -- We can use the RedrawDisplay() method to clear the screen and redraw
  -- the tilemap in a single call.
  RedrawDisplay()

	--Clear canvas
  canvas:Clear(0)

	--Draw canvas particles
  for k, p in pairs(canvasparticles) do
    p:draw()
  end

	--Draw canvas
  canvas:DrawPixels()

	--Draw sprite particles
  for k, p in pairs(spriteparticles) do
    p:draw()
  end
end
