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

    // =============================
    //  RANDOM / WEIGHTED CREATION
    // =============================
    public GameObject CreateRandomCard(Vector3 position, float attackChance, float defenseChance, float healChance, bool forPlayer = true)
    {
        CardData data = library.GetRandomCardWeighted(attackChance, defenseChance, healChance, forPlayer);

        if (data == null)
        {
            Debug.LogError("❌ CardFactory: No valid card found for this request!");
            return null;
        }

        return CreateCard(data, position);
    }

    // =============================
    //  DIRECT CREATION BY ID / NAME
    // =============================
    public GameObject PullCardById(int id, Vector3 position)
    {
        CardData data = library.GetById(id);
        return CreateCard(data, position);
    }

    public GameObject PullCardByName(string name, Vector3 position)
    {
        CardData data = library.GetByName(name);
        return CreateCard(data, position);
    }

    // =============================
    //  INTERNAL CREATION
    // =============================
    private GameObject CreateCard(CardData data, Vector3 position)
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
