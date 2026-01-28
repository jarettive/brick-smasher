using UnityEngine;

/// <summary>
/// Defines the play area with walls on left and right sides.
/// Top and bottom are open for bricks to be launched out.
/// </summary>
public class Area : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D leftWall;

    [SerializeField]
    private BoxCollider2D rightWall;

    public BoxCollider2D LeftWall => leftWall;
    public BoxCollider2D RightWall => rightWall;

    public Rect GetBounds()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            return new Rect();
        return rectTransform.rect;
    }
}
