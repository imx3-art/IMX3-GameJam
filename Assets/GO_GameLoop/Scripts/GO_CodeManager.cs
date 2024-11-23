using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CodeManager : NetworkBehaviour
{
    public static GO_CodeManager Instance; // Singleton para acceso global.

    [Networked] private string generatedCode { get => default; set { } } // Código aleatorio generado.
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
        // Genera un código de 4 dígitos como string.
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
        // Generamos el código completo de 4 dígitos
        char[] codeDigits = generatedCode.ToCharArray();

        // Creamos una lista para representar el código que se mostrará en el input (con _ donde están los libros)
        List<char> inputCode = new List<char>(codeDigits);

        // Lista de índices asignados a los libros
        List<int> assignedIndexes = new List<int>();

        // Asignamos los números de los libros a las posiciones correspondientes
        for (int i = 0; i < predefinedBooks.Count; i++)
        {
            if (i < codeDigits.Length && predefinedBooks[i] != null)
            {
                predefinedBooks[i].SetActive(true); // Activamos el libro si está desactivado

                GO_InteractableBook bookScript = predefinedBooks[i].GetComponent<GO_InteractableBook>();
                if (bookScript != null)
                {
                    // Asigna el número del libro basado en la posición del código
                    bookScript.SetNumber(codeDigits[i].ToString(), i, positionColors[i]);
                    assignedIndexes.Add(i); // Guardamos el índice asignado
                }
                else
                {
                    Debug.LogError($"El libro {predefinedBooks[i].name} no tiene asignado un script GO_InteractableBook.");
                }
            }
        }

        // Generar el código visible para el input
        for (int i = 0; i < inputCode.Count; i++)
        {
            // Si el índice está asignado a un libro, reemplazamos con '_'
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
        // Valida si el código ingresado por el usuario coincide con el generado.
        return userInput == generatedCode;
    }
}
