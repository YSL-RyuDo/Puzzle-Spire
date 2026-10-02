using UnityEngine;

namespace PuzzleSpire.Hotel
{
    // Normal play uses the unscaled clock. Offline video uses fixed frame time so
    // slow disk/encoding work cannot speed up or skip the visible animations.
    public static class HotelPresentationClock
    {
        public static bool Recording { get; set; }
        public static float Now => Recording ? Time.time : Time.unscaledTime;
        public static float Delta => Recording ? Time.deltaTime : Time.unscaledDeltaTime;
    }
}
