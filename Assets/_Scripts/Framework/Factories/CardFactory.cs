// spawns prefab instances using the correct carddata

using UnityEngine;

public class CardFactory : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private GameObject defensePrefab;
    [SerializeField] private GameObject healingPrefab;

    [Header("Data")]
    [SerializeField] private CardLibrary library;


    private void Awake()
    {
        if (library != null)
            library.Initialize();
    }

    public GameObject CreateRandomCard(Vector3 position, float attackChance, float defenseChance, float healChance)
    {
        CardData data = library.GetRandomCardWeighted(attackChance, defenseChance, healChance);
        return CreateCard(data, position);
    }

    public GameObject CreateCard(CardData data, Vector3 position)
    {
        if (data == null)
        {
            Debug.LogError("❌ CardFactory: Tried to create null card data!");
            return null;
        }

        GameObject prefab = data.cardType switch
        {
            CardType.Attack => attackPrefab,
            CardType.Defense => defensePrefab,
            CardType.Healing => healingPrefab,
            _ => null
        };

        if (prefab == null)
        {
            Debug.LogError($"❌ CardFactory: Missing prefab for {data.cardType}!");
            return null;
        }

        GameObject cardObj = Instantiate(prefab, position, Quaternion.identity);
        var runner = cardObj.GetComponent<CardRunner>();
        if (runner != null)
            runner.data = data;

        return cardObj;
    }
}
