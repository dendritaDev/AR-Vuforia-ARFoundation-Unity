using UnityEngine;
using TMPro;


[System.Serializable]
public struct ModelInfo
{
    public GameObject model;
    public string description;
    public bool isActivated;
}
public class UpdateModelInfo : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _description;

    public void ReceiveData(ModelInfo data)
    {
        _title.text = data.model.name;
        _description.text = data.description;
    }
}
