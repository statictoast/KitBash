using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YokuPlatform : Platform
{
    private bool m_IsActive = true;

    public void SetActive(bool aActive)
    {
        if(m_IsActive != aActive)
        {
            m_IsActive = aActive;
            gameObject.SetActive(aActive);
        }
    }
}
