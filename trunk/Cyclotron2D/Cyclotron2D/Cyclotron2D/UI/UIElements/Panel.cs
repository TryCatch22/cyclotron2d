using System.Collections.Generic;
using Cyclotron2D.Screens.Base;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI.UIElements
{
    public abstract class Panel : UIElement
    {
        protected List<UIElement> Items { get; private set; }

        public int ItemSpacing { get; set; }

        protected Panel(Game game, Screen screen) : base(game, screen)
        {
            Items = new List<UIElement>();
            ItemSpacing = 2; //default val
            UpdateOrder = 250;//generally after other UI elements.
        }

        public virtual void AddItems(params UIElement[] item)
        {
            Items.AddRange(item);
        }

        public virtual void RemoveItem(params UIElement[] items)
        {
            foreach (var item in items)
            {
                Items.Remove(item);
            }   

        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing && Items != null)
            {
                foreach (UIElement element in Items)
                {
                    element.Dispose();
                }
                Items = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
