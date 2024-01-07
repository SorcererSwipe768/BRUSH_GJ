using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingAnvil : MonoBehaviour
{
    [SerializeField] private List<CraftingRecipeSO> craftingRecipeSOList;
    [SerializeField] private BoxCollider placeItemsAreaBoxCollider;
    [SerializeField] private Transform itemSpawnPoint;

    [SerializeField] private Transform vfxSpawnItem;

    private CraftingRecipeSO craftingRecipeSO;

    private void Awake()
    {
        craftingRecipeSO = craftingRecipeSOList[0];
    }

    public void Craft()
    {
        Debug.Log("craft");
        Collider[] colliderArray = Physics.OverlapBox(transform.position + placeItemsAreaBoxCollider.center, placeItemsAreaBoxCollider.size, placeItemsAreaBoxCollider.transform.rotation);

        List<ItemSO> inputItemList = new List<ItemSO>(craftingRecipeSO.inputItemSOList);
        List<GameObject> consumeItemGameObjectList = new List<GameObject>();

        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out ItemSOHolder itemSOHolder))
            {
                if (inputItemList.Contains(itemSOHolder.itemSO))
                {
                    inputItemList.Remove(itemSOHolder.itemSO);
                    consumeItemGameObjectList.Add(collider.gameObject);
                }
            }
        }

        if (inputItemList.Count == 0)
        {
            Debug.Log("yay workie");
            Transform spawnedItemTransform = Instantiate(craftingRecipeSO.outputItemSO.prefab, itemSpawnPoint.position, itemSpawnPoint.rotation);

            Instantiate(vfxSpawnItem, itemSpawnPoint.position, itemSpawnPoint.rotation);

            foreach (GameObject consumeItemGameObject in consumeItemGameObjectList)
            {
                Destroy(consumeItemGameObject);
            }
        } 




    }
}
