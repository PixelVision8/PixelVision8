using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public partial class ControllerChip
    {
        private List<Controller> players;
        private readonly GamePadDeadZone gamePadDeadZone = GamePadDeadZone.IndependentAxes;

        public bool IsConnected(int id)
        {
            return players[id].IsConnected();
        }

        public void RegisterControllers()
        {
            var state = GamePad.GetState(0, gamePadDeadZone);
            if (state.IsConnected)
            {
                var player1 = getPlayer(0);
                //                player1.GamePadIndex = 0;
                player1.CurrentState = state;
            }

            state = GamePad.GetState(1, gamePadDeadZone);
            if (state.IsConnected)
            {
                var player2 = getPlayer(1);
                //                player2.GamePadIndex = 1;
                player2.CurrentState = state;
            }
        }
    }
}