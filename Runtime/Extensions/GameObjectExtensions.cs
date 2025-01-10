using UnityEngine;

namespace skner.DualGrid.Extensions
{
    public static class GameObjectExtensions
    {

        /// <summary>
        /// Returns the first component found in immediate parent of <paramref name="gameObject"/>.
        /// </summary>
        /// <remarks>
        /// It will not return a component found in the <paramref name="gameObject"/>'s game object.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetComponentInImmediateParent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.transform?.parent?.GetComponent<T>();
        }

    }
}
