

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public class MouseInputChip : AbstractChip, IUpdate
    {
        public MouseState currentMouseState;
        public MouseState previousMouseState;
        
        protected override void Configure()
        {
            Player.MouseInputChip = this;

            // Setup Mouse
            currentMouseState = Mouse.GetState();
            previousMouseState = currentMouseState;

        }
        
        public void Update(int timeDelta)
        {
            
            // Save the one and only (if available) mousestate 
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }
        
        private bool IsPressed(MouseState state, int input)
        {
            switch (input)
            {
                case 0:
                    return state.LeftButton == ButtonState.Pressed;
                case 1:
                    return state.RightButton == ButtonState.Pressed;
            }

            return false;
        }

        #region Mouse APIs

        public bool GetMouseButtonDown(int id = 0)
        {
            return IsPressed(currentMouseState, id) && IsPressed(previousMouseState, id);
        }

        public bool GetMouseButtonUp(int id = 0)
        {
            return !IsPressed(currentMouseState, id) && IsPressed(previousMouseState, id);
        }

        public Point ReadMousePosition()
        {
            var pos = PointToScreen(currentMouseState.Position.X, currentMouseState.Position.Y);
            return new Point(pos.X, pos.Y);
        }

        protected Point mouseWheelPos = new Point();

        public Point ReadMouseWheel()
        {
            mouseWheelPos.X = currentMouseState.HorizontalScrollWheelValue -
                              previousMouseState.HorizontalScrollWheelValue;
            mouseWheelPos.Y = currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;

            return mouseWheelPos;
        }

        private Matrix scaleMatrix = Matrix.CreateScale(1, 1, 1);

        public void MouseScale(float x, float y)
        {
            scaleMatrix = Matrix.CreateScale(x, y, 1.0f);
        }

        // public Point PointToScreen(int x, int y)
        // {
        //     return PointToScreen(point.X, point.Y);
        // }

        public Point PointToScreen(int x, int y)
        {
            var vx = x;
            var vy = y;
            var invertedMatrix = Matrix.Invert(scaleMatrix);
            var vector = Vector2.Transform(new Vector2(vx, vy), invertedMatrix);
            return new Point((int)vector.X, (int)vector.Y);
        }
        
        #endregion

    }
    
    public partial class PixelVision
    {
        public MouseInputChip MouseInputChip { get; set; }
    }
}