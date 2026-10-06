using System;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    // These are references to authored scene Images, never runtime-created UI.
    public sealed class HotelThemeView : MonoBehaviour
    {
        [Serializable] public sealed class Binding
        {
            public string key;
            public Image image;
            public Sprite defaultSprite;
        }
        public Binding[] backgrounds = Array.Empty<Binding>();
        public void Apply(HotelStorage storage)
        {
            foreach (var binding in backgrounds)
                if (binding.image) binding.image.sprite = storage.Image(binding.key) ?? binding.defaultSprite;
        }
    }
    public static class HotelThemeKeys
    {
        public static readonly string[] Pieces = {
            "block.0", "block.1", "block.2", "block.3", "block.4",
            "special.horizontal", "special.vertical", "special.bomb", "special.color",
            "obstacle.box", "obstacle.box.damaged", "obstacle.ice", "obstacle.ice.damaged",
            "clue.thread", "clue.key", "clue.towel"
        };
    }
}
