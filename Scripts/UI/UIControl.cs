using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    // TODO: cache off bouding box and only change it when marked to change it
    public float GetWidth()
    {
        Bounds boundingBox = GetBoundingBox();
        return boundingBox.extents.x * 2;
    }

    public float GetHeight()
    {
        Bounds boundingBox = GetBoundingBox();
        return boundingBox.extents.y * 2;
    }

    public Bounds GetBoundingBox()
    {
        Vector4 aabb = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
        GetAABBValues(ref aabb);

        Bounds boundingBox = new Bounds();
        boundingBox.SetMinMax(new Vector3(aabb.x, aabb.y), new Vector3(aabb.z, aabb.w));

        return boundingBox;
    }

    protected void GetAABBValues(ref Vector4 aBoundingBox)
    {
        RectTransform[] otherthings = gameObject.GetComponentsInChildren<RectTransform>();

        foreach (RectTransform rect in GetComponentsInChildren<RectTransform>())
        {
            Vector3[] v = new Vector3[4];
            rect.GetWorldCorners(v);

            for (int i = 0; i < 4; i++)
            {
                Vector3 corner = v[i];
                if (corner.x < aBoundingBox.x)
                {
                    aBoundingBox.x = corner.x;
                }

                if (corner.x > aBoundingBox.z)
                {
                    aBoundingBox.z = corner.x;
                }

                if (corner.y < aBoundingBox.y)
                {
                    aBoundingBox.y = corner.y;
                }

                if (corner.y > aBoundingBox.w)
                {
                    aBoundingBox.w = corner.y;
                }
            }
        }
    }

    public void SetShowing(bool aShowing)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup)
        {
            canvasGroup.alpha = aShowing ? 1f : 0f;
        }
        else
        {
            Image[] images = GetComponentsInChildren<Image>();
            foreach(Image image in images)
            {
                image.enabled = aShowing;
            }

            Text[] textFields = GetComponentsInChildren<Text>();
            foreach(Text text in textFields)
            {
                text.enabled = aShowing;
            }
        }
    }

    public void SetAlpha(float aAlpha)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup)
        {
            canvasGroup.alpha = aAlpha;
        }
    }
}
