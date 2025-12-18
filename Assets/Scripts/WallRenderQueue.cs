using UnityEngine;
using System.Collections.Generic;

public class WallRenderQueue : MonoBehaviour
{
    [SerializeField] List<GameObject> walls = new();
    private readonly List<Renderer> _renderers = new List<Renderer>();
    private List<bool> behind = new();

    void Start()
    {
        foreach (var wall in walls)
            _renderers.Add(wall.GetComponent<Renderer>());

        if (walls.Count == 4)
            behind = new List<bool> { true, true, false, false };
        else behind = new List<bool> { true, true, true, true, false, false };
        ChangeRenderQueue(behind);
    }

    public void RenderAtPos(int pos)
    {
        if (walls.Count == 4)
        {
            switch (pos)
            {
                case 0:
                    behind = new List<bool> { true, false, false, true };
                    break;

                case 1:
                    behind = new List<bool> { false, false, true, true };
                    break;

                case 2:
                    behind = new List<bool> { false, true, true, false };
                    break;

                case 3:
                    behind = new List<bool> { true, true, false, false };
                    break;
            }
            ChangeRenderQueue(behind);
        }
        else
        {
            switch (pos)
            {
                case 0:
                    behind = new List<bool> { true, true, true, false, false, true};
                    break;

                case 1:
                    behind = new List<bool> { false, false, false, false, true, true};
                    break;

                case 2:
                    behind = new List<bool> { false, false, false, true, true, false};
                    break;

                case 3:
                    behind = new List<bool> { true, true, true, true, false, false};
                    break;
            }
            ChangeRenderQueue(behind);
        }       
    }

    private void ChangeRenderQueue(List<bool> behind)
    {
        for (int i = 0; i < walls.Count; i++)
        {
            if (behind[i]) _renderers[i].material.renderQueue = 3000;
            else _renderers[i].material.renderQueue = 3200;
        }
    }
}
