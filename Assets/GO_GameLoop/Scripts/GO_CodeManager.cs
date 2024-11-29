using System;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GO_CodeManager : NetworkBehaviour
{
    [Networked, Capacity(10)] public NetworkArray<string> generatedCodes { get; }
    public List<GameObject> predefinedBooks; // Lista de libros predefinidos en la escena.
    [SerializeField] private List<Color> positionColors;
    public static string displayedCode;

    private void Start()
    {
        int currentLevelIndex = GetCurrentLevelIndex();

        // Solo el nodo con autoridad genera el c�digo si a�n no est� definido.
        if (Object.HasStateAuthority && string.IsNullOrEmpty(generatedCodes[currentLevelIndex]))
        {
            GenerateCode(currentLevelIndex);
        }

        // Asegurarse de distribuir el c�digo actual.
        StartCoroutine(WaitAndDistributeCode(currentLevelIndex));
    }

    private int GetCurrentLevelIndex()
    {
        // Aqu� puedes implementar c�mo obtener el �ndice del nivel actual.
        // Por simplicidad, asumimos que los niveles est�n indexados desde 0.
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }

    public void GenerateCode(int levelIndex)
    {
        // Genera un c�digo de 4 d�gitos para el nivel actual si no est� definido.
        if (string.IsNullOrEmpty(generatedCodes[levelIndex]))
        {
            generatedCodes.Set(levelIndex, Random.Range(1000, 9999).ToString());
        }
    }

    private IEnumerator WaitAndDistributeCode(int levelIndex)
    {
        // Espera a que el c�digo del nivel actual est� sincronizado en todos los nodos.
        while (string.IsNullOrEmpty(generatedCodes[levelIndex]))
        {
            yield return null; // Espera un frame.
        }

        DistributeCode(levelIndex);
    }

    public void DistributeCode(int levelIndex)
    {
        // Obt�n el c�digo del nivel actual.
        string code = generatedCodes[levelIndex];

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning($"El c�digo para el nivel {levelIndex} no est� definido.");
            return;
        }

        // Genera los d�gitos del c�digo como una lista de caracteres.
        char[] codeDigits = code.ToCharArray();

        // Crea la lista para el input, reemplazando con '_' donde est�n los libros.
        List<char> inputCode = new List<char>(codeDigits);

        // Lista de �ndices asignados a los libros.
        List<int> assignedIndexes = new List<int>();

        // Asigna los n�meros a los libros.
        for (int i = 0; i < predefinedBooks.Count; i++)
        {
            if (i < codeDigits.Length && predefinedBooks[i] != null)
            {
                predefinedBooks[i].SetActive(true); // Activa el libro si est� desactivado.

                GO_InteractableBook bookScript = predefinedBooks[i].GetComponent<GO_InteractableBook>();
                if (bookScript != null)
                {
                    bookScript.SetNumber(codeDigits[i].ToString(), i, positionColors[i]);
                    assignedIndexes.Add(i); // Guarda el �ndice asignado.
                }
                else
                {
                    Debug.LogError($"El libro {predefinedBooks[i].name} no tiene un script GO_InteractableBook.");
                }
            }
        }

        for (int i = 0; i < inputCode.Count; i++)
        {
            if (assignedIndexes.Contains(i))
            {
                inputCode[i] = '_';
            }
        }

        displayedCode = new string(inputCode.ToArray());
    }

    public bool ValidateCode(string userInput)
    {
        int currentLevelIndex = GetCurrentLevelIndex();
        bool isOk = userInput == generatedCodes[currentLevelIndex];
        Debug.Log(isOk);
        return isOk;
    }
}
