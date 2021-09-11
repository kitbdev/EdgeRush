using UnityEngine;
using System.Collections.Generic;
/*
code by: bellicapax
https://forum.unity.com/threads/is-there-a-way-to-get-the-layer-collision-matrix.260744/#post-3483886
*/
public static class PhysicsCollisionMatrixLayerMasks
{
    private static Dictionary<int, int> _masksByLayer;

    private static void Init()
    {
        _masksByLayer = new Dictionary<int, int>();
        for (int i = 0; i < 32; i++)
        {
            int mask = 0;
            for (int j = 0; j < 32; j++)
            {
                if (!Physics.GetIgnoreLayerCollision(i, j))
                {
                    mask |= 1 << j;
                }
            }
            _masksByLayer.Add(i, mask);
        }
    }

    public static int MaskForLayer(int layer)
    {
        if (_masksByLayer == null || _masksByLayer.Count == 0)
        {
            Init();
        }
        return _masksByLayer[layer];
    }
}