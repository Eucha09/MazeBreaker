using System.Collections.Generic;
using UnityEngine;
using static Define;

public class LayerManager
{
    int _excludedLayerMask;

    public int ExcludedLayerMask { get { return _excludedLayerMask; } }

    public void Init()
    {
        _excludedLayerMask = 0;

        ExcludeLayer((int)Layer.DRM);
    }

    public void Clear()
    {

    }

    public void ExcludeLayer(int layer)
    {
        _excludedLayerMask |= 1 << layer;
    }

    public void IncludeLayer(int layer)
    {
        _excludedLayerMask &= ~(1 << layer);
    }

    public bool IsLayerExcluded(int layer)
    {
        return (_excludedLayerMask & (1 << layer)) > 0;
    }
}
