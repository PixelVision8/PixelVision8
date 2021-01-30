using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace PixelVision8.Player
{
    public enum MouseInput
    {
        LeftButton,
        RightButton,
        MiddleButton,
        Button1,
        Button2,
        None
    }

    public partial class ControllerChip
    {
        public MouseState currentMouseState;
        public MouseState previousMouseState;

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
            var pos = PointToScreen(currentMouseState.Position);
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

        public Point PointToScreen(Point point)
        {
            return PointToScreen(point.X, point.Y);
        }

        public Point PointToScreen(int x, int y)
        {
            var vx = x;
            var vy = y;
            var invertedMatrix = Matrix.Invert(scaleMatrix);
            return Vector2.Transform(new Vector2(vx, vy), invertedMatrix).ToPoint();
        }

        #endregion
    }
}