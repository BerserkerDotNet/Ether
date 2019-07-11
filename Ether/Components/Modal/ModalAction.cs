using System;

namespace Ether.Components.Modal
{
    public class ModalAction
    {
        private ModalAction(string displayName, Action onClick)
        {
            DisplayName = displayName;
            OnClick = onClick;
        }

        public string DisplayName { get; private set; }

        public Action OnClick { get; private set; }

        public static ModalAction Get(string displayName, Action onClick) => new ModalAction(displayName, onClick);

        public static ModalAction GetClose() => new ModalAction("Close", null);
    }
}
