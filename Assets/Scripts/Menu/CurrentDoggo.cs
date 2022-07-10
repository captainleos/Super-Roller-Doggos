using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentDoggo : MonoBehaviour
{
    [SerializeField] public List<GameObject> doggos = new List<GameObject>();
    [SerializeField] public int selectedDoggo = 0;

    private void Start()
    {
        selectedDoggo = MainManager.Instance.selectedDoggo;
        ShowSelectedDoggo(selectedDoggo);
    }

    public void ShowSelectedDoggo(int selectedDoggo)
    {
        for (int i = 0; i < doggos.Count; ++i)
        {
            doggos[i].SetActive(false);
        }
        doggos[selectedDoggo].SetActive(true);
    }
}
