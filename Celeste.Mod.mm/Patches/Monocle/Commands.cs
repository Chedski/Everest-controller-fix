﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monocle {
    class patch_Commands : Commands {

        // We're effectively in Commands, but still need to "expose" private fields to our mod.
        private bool canOpen;
        private KeyboardState currentState;
        private bool underscore;
        private string currentText = "";
        private List<patch_Line> drawCommands;

        private Vector2 mousePosition;

        [MonoModReplace] // Don't create orig_ method.
        internal void UpdateClosed() {
            if (!canOpen) {
                canOpen = true;
            // Original code only checks OemTillde and Oem8, leaving QWERTZ users in the dark...
            } else if (MInput.Keyboard.Pressed(Keys.OemTilde, Keys.Oem8, Keys.OemPeriod)) {
                Open = true;
                currentState = Keyboard.GetState();
            }

            // Execute F-key actions.
            for (int i = 0; i < FunctionKeyActions.Length; i++)
                if (MInput.Keyboard.Pressed(Keys.F1 + i))
                    ExecuteFunctionKeyAction(i);
        }

        [MonoModReplace]
        internal void Render() {
            // Vector2 mousePosition = MInput.Mouse.Position;
            // For whatever reason, MInput.Mouse.Position keeps returning 0, 0
            // Let's just use the XNA / FNA MouseState instead.
            MouseState mouseState = Mouse.GetState();
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            string mouseText = $"Cursor @\n raw: {(int) Math.Round(mousePosition.X)}, {(int) Math.Round(mousePosition.Y)}";

            if (Engine.Scene is Level) {
                Level level = (Level) Engine.Scene;
                Camera cam = level.Camera;
                Vector2 mouseWorldPosition;
                mouseWorldPosition = cam.ScreenToCamera(mousePosition);
                mouseText += $"\n world: {(int) Math.Round(mouseWorldPosition.X)}, {(int) Math.Round(mouseWorldPosition.Y)}";
                mouseWorldPosition -= level.LevelOffset;
                mouseText += $"\n level: {(int) Math.Round(mouseWorldPosition.X)}, {(int) Math.Round(mouseWorldPosition.Y)}";
            }

            int viewWidth = Engine.ViewWidth;
            int viewHeight = Engine.ViewHeight;

            Draw.SpriteBatch.Begin();

            // Draw cursor below all UI.
            Draw.Line(mousePosition.X - 4f, mousePosition.Y, mousePosition.X + 3f, mousePosition.Y, Color.Yellow);
            Draw.Line(mousePosition.X, mousePosition.Y - 3f, mousePosition.X, mousePosition.Y + 4f, Color.Yellow);

            // Draw cursor world position.
            Vector2 mouseTextSize = Draw.DefaultFont.MeasureString(mouseText);
            Draw.Rect(10f, 10f, mouseTextSize.X + 20f, mouseTextSize.Y + 20f, Color.Black * 0.8f);
            Draw.SpriteBatch.DrawString(
                Draw.DefaultFont,
                mouseText,
                new Vector2(20f, 20f),
                Color.White
            );

            // Draw standard console.

            Draw.Rect(10f, viewHeight - 50f, viewWidth - 20f, 40f, Color.Black * 0.8f);

            if (underscore) {
                Draw.SpriteBatch.DrawString(
                    Draw.DefaultFont,
                    ">" + currentText + "_",
                    new Vector2(20f, viewHeight - 42f),
                    Color.White
                );
            } else {
                Draw.SpriteBatch.DrawString(
                    Draw.DefaultFont,
                    ">" + currentText,
                    new Vector2(20f, viewHeight - 42f),
                    Color.White
                );
            }

            if (drawCommands.Count > 0) {
                float height = 10f + 30f * drawCommands.Count;
                Draw.Rect(10f, viewHeight - height - 60f, viewWidth - 20f, height, Color.Black * 0.8f);
                for (int i = 0; i < drawCommands.Count; i++) {
                    Draw.SpriteBatch.DrawString(
                        Draw.DefaultFont,
                        drawCommands[i].Text,
                        new Vector2(20f, viewHeight - 92f - 30f * i),
                        drawCommands[i].Color
                    );
                }
            }

            Draw.SpriteBatch.End();
        }

        // Only required to be defined so that we can access it.
        [MonoModIgnore]
        private struct patch_Line {
            public string Text;
            public Color Color;
            public patch_Line(string text) {
                Text = text;
                Color = Color.White;
            }
            public patch_Line(string text, Color color) {
                Text = text;
                Color = color;
            }
        }

    }
}
