using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpGuide : MonoBehaviour
{
    LineRenderer renderer;

    public bool DrawEnabled
    {
        get { return renderer.enabled; }
    }

    // Start is called before the first frame update
    void Start()
    { 
        renderer = gameObject.GetComponent<LineRenderer>();
        // 線の幅
        renderer.SetWidth(0.1f, 0.1f);
        // 頂点の数
        renderer.SetVertexCount(2);
        // 頂点を設定
    }
    
    public void DrawLine(Vector2 start, Vector2 end)
    {
        renderer.enabled = true;
        renderer.SetPosition(0, start);
        renderer.SetPosition(1, end);
    }

    public void HideLine()
    {
        renderer.enabled = false;
    }
}
