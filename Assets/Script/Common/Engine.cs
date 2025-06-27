using UnityEngine;

namespace Common
{
    public class Engine
    {
    }

    public static class Tools
    {
        public static bool Cover(int direction, float current, float end)
        {
            return direction switch
            {
                0 => true,
                1 => current >= end,
                _ => current <= end
            };
        }
    }
}