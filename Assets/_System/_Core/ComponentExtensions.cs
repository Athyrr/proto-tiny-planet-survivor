using UnityEngine;

public static class ComponentExtensions
{

    public static T GetComponentInParents<T>(this Component self, bool includeInactive = false) where T : Component
    {
        if (self == null)
            return null;
        Transform current = self.transform.parent;

        while (current != null)
        {
            T component = current.GetComponent<T>();
            if (component != null && (includeInactive || component.gameObject.activeInHierarchy))
            {
                return component;
            }
            current = current.parent;
        }

        return default;
    }

    public static bool TryGetComponentInParents<T>(this Component self, out T component, bool includeInactive = false) where T : Component
    {
        component = self.GetComponentInParents<T>(includeInactive);
        return component != null;
    }
}
