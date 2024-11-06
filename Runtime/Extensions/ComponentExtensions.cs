using UnityEngine;

namespace skner.DualGrid.Extensions
{
    public static class ComponentExtensions
    {

        /// <summary>
        /// Returns the first component found in immediate children of <paramref name="parent"/>.
        /// </summary>
        /// <remarks>
        /// It will not return a component found in the <paramref name="parent"/>'s game object.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T GetComponentInImmediateChildren<T>(this Component parent) where T : Component
        {
            foreach (Transform child in parent.transform)
            {
                T component = child.GetComponent<T>();
                if (component != null && component.transform != parent.transform)
                {
                    return component;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the first component found in immediate children of <paramref name="component"/>.
        /// </summary>
        /// <remarks>
        /// It will not return a component found in the <paramref name="component"/>'s game object.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static T GetComponentInImmediateParent<T>(this Component component) where T : Component
        {
            return component.transform?.parent?.GetComponent<T>();
        }

    }
}
