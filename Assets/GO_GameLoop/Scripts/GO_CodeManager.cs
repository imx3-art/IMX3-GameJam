using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CodeManager : NetworkBehaviour
{
    [Networked, Capacity(10)] public NetworkArray<string> generatedCodes { get; }
    public List<GameObject> predefinedBooks; // Lista de libros predefinidos en la escena.
    [SerializeField] private List<Color> positionColors;
    public static string displayedCode;

    private void Start()
    {
        int currentLevelIndex = GetCurrentLevelIndex();

        // Solo el nodo con autoridad genera el código si aún no está definido.
        if (Object.HasStateAuthority && string.IsNullOrEmpty(generatedCodes[currentLevelIndex]))
        {
            GenerateCode(currentLevelIndex);
        }

        // Asegurarse de distribuir el código actual.
        StartCoroutine(WaitAndDistributeCode(currentLevelIndex));
    }

    private int GetCurrentLevelIndex()
    {
        // Aquí puedes implementar cómo obtener el índice del nivel actual.
        // Por simplicidad, asumimos que los niveles están indexados desde 0.
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }

    public void GenerateCode(int levelIndex)
    {
        // Genera un código de 4 dígitos para el nivel actual si no está definido.
        if (string.IsNullOrEmpty(generatedCodes[levelIndex]))
        {
            generatedCodes.Set(levelIndex, Random.Range(1000, 9999).ToString());
            Debug.Log($"Código nuevo para el nivel {levelIndex}: {generatedCodes[levelIndex]}");
        }
        else
        {
            Debug.Log($"Código existente para el nivel {levelIndex}: {generatedCodes[levelIndex]}");
        }
    }

    private IEnumerator WaitAndDistributeCode(int levelIndex)
    {
        // Espera a que el código del nivel actual esté sincronizado en todos los nodos.
        while (string.IsNullOrEmpty(generatedCodes[levelIndex]))
        {
            yield return null; // Espera un frame.
        }

        DistributeCode(levelIndex);
    }

    public void DistributeCode(int levelIndex)
    {
        // Obtén el código del nivel actual.
        string code = generatedCodes[levelIndex];

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning($"El código para el nivel {levelIndex} no está definido.");
            return;
        }

        // Genera los dígitos del código como una lista de caracteres.
        char[] codeDigits = code.ToCharArray();

        // Crea la lista para el input, reemplazando con '_' donde están los libros.
        List<char> inputCode = new List<char>(codeDigits);

        // Lista de índices asignados a los libros.
        List<int> assignedIndexes = new List<int>();

        // Asigna los números a los libros.
        for (int i = 0; i < predefinedBooks.Count; i++)
        {
            if (i < codeDigits.Length && predefinedBooks[i] != null)
            {
                predefinedBooks[i].SetActive(true); // Activa el libro si está desactivado.

                GO_InteractableBook bookScript = predefinedBooks[i].GetComponent<GO_InteractableBook>();
                if (bookScript != null)
                {
                    bookScript.SetNumber(codeDigits[i].ToString(), i, positionColors[i]);
                    assignedIndexes.Add(i); // Guarda el índice asignado.
                }
                else
                {
                    Debug.LogError($"El libro {predefinedBooks[i].name} no tiene un script GO_InteractableBook.");
                }
            }
        }

        // Genera el código visible para el input.
        for (int i = 0; i < inputCode.Count; i++)
        {
            if (assignedIndexes.Contains(i))
            {
                inputCode[i] = '_';
            }
        }

        // Convierte la lista de caracteres en una cadena para mostrar.
        displayedCode = new string(inputCode.ToArray());
        Debug.Log($"Código distribuido para el nivel {levelIndex}: {displayedCode}");
    }

    public bool ValidateCode(string userInput)
    {
        // Obtén el índice del nivel actual.
        int currentLevelIndex = GetCurrentLevelIndex();
        Debug.Log("CONTRASAEÑA" + generatedCodes[currentLevelIndex]);
        Debug.Log("CURRENT LEVEL" + currentLevelIndex);
        Debug.Log("USERINPUT" + userInput);
        // Valida si el código ingresado coincide con el generado para el nivel actual.
        bool isOk = userInput == generatedCodes[currentLevelIndex];
        Debug.Log(isOk);
        return isOk;
    }
}
