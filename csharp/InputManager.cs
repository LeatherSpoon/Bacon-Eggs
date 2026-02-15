using System.Collections.Generic;

namespace BaconEggs
{
    /// <summary>
    /// The keyboard keys used by the game (WASD movement).
    /// Converted from eventListeners.js.
    /// </summary>
    public enum GameKey
    {
        W,
        A,
        S,
        D
    }

    /// <summary>
    /// Tracks which movement keys are currently pressed.
    /// Converted from the window keydown/keyup event listeners in eventListeners.js.
    ///
    /// Usage:
    ///   var input = new InputManager();
    ///   // In your framework's KeyDown callback:
    ///   input.OnKeyDown(GameKey.D);
    ///   // In your framework's KeyUp callback:
    ///   input.OnKeyUp(GameKey.D);
    ///   // In the game loop:
    ///   player.HandleInput(input);
    /// </summary>
    public class InputManager
    {
        private readonly Dictionary<GameKey, bool> _pressed = new Dictionary<GameKey, bool>
        {
            { GameKey.W, false },
            { GameKey.A, false },
            { GameKey.S, false },
            { GameKey.D, false },
        };

        /// <summary>Returns true if the given key is currently held down.</summary>
        public bool IsKeyPressed(GameKey key)
        {
            return _pressed.TryGetValue(key, out bool state) && state;
        }

        /// <summary>
        /// Call this when a key-down event is received from your framework.
        /// Maps raw key strings ("w","a","s","d") to <see cref="GameKey"/> values.
        /// </summary>
        public void OnKeyDown(string key)
        {
            switch (key.ToLowerInvariant())
            {
                case "w": _pressed[GameKey.W] = true; break;
                case "a": _pressed[GameKey.A] = true; break;
                case "s": _pressed[GameKey.S] = true; break;
                case "d": _pressed[GameKey.D] = true; break;
            }
        }

        /// <summary>
        /// Call this when a key-up event is received from your framework.
        /// </summary>
        public void OnKeyUp(string key)
        {
            switch (key.ToLowerInvariant())
            {
                case "w": _pressed[GameKey.W] = false; break;
                case "a": _pressed[GameKey.A] = false; break;
                case "s": _pressed[GameKey.S] = false; break;
                case "d": _pressed[GameKey.D] = false; break;
            }
        }

        /// <summary>
        /// Typed overload for direct framework integration (e.g. with an enum or constant).
        /// </summary>
        public void OnKeyDown(GameKey key) => _pressed[key] = true;

        /// <summary>
        /// Typed overload for direct framework integration.
        /// </summary>
        public void OnKeyUp(GameKey key) => _pressed[key] = false;

        /// <summary>
        /// Call when the application regains focus to prevent stuck keys.
        /// Mirrors the visibilitychange handler in eventListeners.js.
        /// </summary>
        public void ResetAllKeys()
        {
            foreach (var k in System.Enum.GetValues(typeof(GameKey)))
            {
                _pressed[(GameKey)k] = false;
            }
        }
    }
}
