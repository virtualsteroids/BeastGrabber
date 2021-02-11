using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace BeastGrabber
{
    public class BeastGrabber : BaseSettingsPlugin<BeastGrabberSettings>
    {
        private Coroutine worker;
        private const string coroutineName = "Move";
        private bool running = false;
        private static List<Vector2> usedInventorySlots = new List<Vector2>();

        public BeastGrabber()
        {
            Name = "BeastGrabber";
        }
        public override bool Initialise()
        {
            Input.RegisterKey(Settings.Hotkey);

            Settings.Hotkey.OnValueChanged += () => { Input.RegisterKey(Settings.Hotkey); };

            return true;
        }

        public override void Render()
        {
            if (Settings.Hotkey.PressedOnce())
            {
                if (!running)
                {
                    running = true;
                    worker = new Coroutine(ProcessItems(), this, coroutineName);
                    Core.ParallelRunner.Run(worker);
                }
            }
        }

        private IEnumerator ProcessItems()
        {
            usedInventorySlots.Clear();

            var currentMousePos = Input.MousePosition;

            var inventory = GameController.Game.IngameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory];

            if (inventory == null)
            {
                running = false;
                yield break;
            }

            NormalInventoryItem beastOrb = null;

            foreach (NormalInventoryItem item in inventory.VisibleInventoryItems)
            {
                if (item.Item.Metadata.Contains("CurrencyItemiseCapturedMonster"))
                {
                    beastOrb = item;
                }

                usedInventorySlots.Add(new Vector2(item.InventPosX, item.InventPosY));
            }

            if (beastOrb == null)
            {
                running = false;
                yield break;
            }

            MoveMouseToElement(beastOrb.GetClientRect().Center);
            yield return new WaitTime(150);
            Input.Click(System.Windows.Forms.MouseButtons.Right);
            yield return new WaitTime(150);
            MoveMouseToElement(currentMousePos);
            yield return new WaitTime(150);
            Input.Click(System.Windows.Forms.MouseButtons.Left);
            yield return new WaitTime(150);

            for (int x = 0; x < 12; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Vector2 slot = new Vector2(x, y);
                    if (!usedInventorySlots.Contains(slot))
                    {
                        var inventoryRect = inventory.InventoryUIElement.GetClientRect();

                        float halfSlotX = inventoryRect.Width / 24f;
                        float halfSlotY = inventoryRect.Height / 10f;
                        float slotWidth = inventoryRect.Width / 12f;
                        float slotHeight = inventoryRect.Height / 5f;

                        Vector2 slotPos = new Vector2(inventoryRect.X + slotWidth * x + halfSlotX, inventoryRect.Y + slotHeight * y + halfSlotX);
                        MoveMouseToElement(slotPos);
                        yield return new WaitTime(150);
                        Input.Click(System.Windows.Forms.MouseButtons.Left);
                        yield return new WaitTime(150);
                        MoveMouseToElement(currentMousePos);
                        running = false;

                        yield break;
                    }
                }
            }
        }

        private void MoveMouseToElement(Vector2 pos)
        {
            Input.SetCursorPos(pos + GameController.Window.GetWindowRectangle().TopLeft);
        }

    }
}
