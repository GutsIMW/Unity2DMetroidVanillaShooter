using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsDropdownInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<TMPro.TMP_Dropdown>().value = QualitySettings.GetQualityLevel(); // Ensure that the GraphicsDropdown value is set to the default graphics settings at start
    }
}
