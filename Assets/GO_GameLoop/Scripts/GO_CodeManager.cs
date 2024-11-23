using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CodeManager : NetworkBehaviour
{
    public static GO_CodeManager Instance; // Singleton para acceso global.

    [Networked] private string generatedCode { get => default; set { } } // C�digo aleatorio generado.
    public List<GameObject> predefinedBooks; // Lista de libros predefinidos en la escena.
    [SerializeField] private List<Color> positionColors;
    public static string displayedCode;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GenerateCode();
    }

    public void GenerateCode()
    {
        // Genera un c�digo de 4 d�gitos como string.
        if(generatedCode == null || generatedCode == string.Empty)
        {
            generatedCode = Random.Range(1000, 9999).ToString();
            DistributeCode();
        }
        else
        {   
            generatedCode = generatedCode;
            DistributeCode();
        }
    }

    public void DistributeCode()
    {
        // Generamos el c�digo completo de 4 d�gitos
        char[] codeDigits = generatedCode.ToCharArray();

        // Creamos una lista para representar el c�digo que se mostrar� en el input (con _ donde est�n los libros)
        List<char> inputCode = new List<char>(codeDigits);

        // Lista de �ndices asignados a los libros
        List<int> assignedIndexes = new List<int>();

        // Asignamos los n�meros de los libros a las posiciones correspondientes
        for (int i = 0; i < predefinedBooks.Count; i++)
        {
            if (i < codeDigits.Length && predefinedBooks[i] != null)
            {
                predefinedBooks[i].SetActive(true); // Activamos el libro si est� desactivado

                GO_InteractableBook bookScript = predefinedBooks[i].GetComponent<GO_InteractableBook>();
                if (bookScript != null)
                {
                    // Asigna el n�mero del libro basado en la posici�n del c�digo
                    bookScript.SetNumber(codeDigits[i].ToString(), i, positionColors[i]);
                    assignedIndexes.Add(i); // Guardamos el �ndice asignado
                }
                else
                {
                    Debug.LogError($"El libro {predefinedBooks[i].name} no tiene asignado un script GO_InteractableBook.");
                }
            }
        }

        // Generar el c�digo visible para el input
        for (int i = 0; i < inputCode.Count; i++)
        {
            // Si el �ndice est� asignado a un libro, reemplazamos con '_'
            if (assignedIndexes.Contains(i))
            {
                inputCode[i] = '_';
            }
        }

        // Convertimos la lista de caracteres en una cadena para mostrar en el input
        displayedCode = new string(inputCode.ToArray());
    }


    public bool ValidateCode(string userInput)
    {
        // Valida si el c�digo ingresado por el usuario coincide con el generado.
        return userInput == generatedCode;
    }
}
