using UnityEngine;

[CreateAssetMenu(fileName = "Product Data", menuName = "ScriptableObjects/Product Data", order = 0)]
public class ProductSO : ScriptableObject
{
    [Header("Product")]
    [SerializeField] private Mesh productMesh;
    [Space]
    [SerializeField] private Material[] productMaterials;
    [SerializeField] private Material[] productMaterialsTransparent;

    public Mesh GetMesh()
    {
        return productMesh;
    }
    
    public Material[] GetMaterials(bool IsPlaced)
    {
        return IsPlaced ? productMaterials : productMaterialsTransparent;
    }

}
