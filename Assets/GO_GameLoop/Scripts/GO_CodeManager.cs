using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CodeManager : NetworkBehaviour
{
    public static GO_CodeManager Instance; // Singleton para acceso global.

    [Networked] private string generatedCode { get => default; set { } } // Código aleatorio generado.
    public Transform[] bookSpawns; // Lugares donde aparecerán los libros.
    public GameObject bookPrefab; // Prefab del librito.

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
        generatedCode = Random.Range(1000, 9999).ToString();
        Debug.Log($"Código generado: {generatedCode}");

        DistributeCode();
    }

    public void DistributeCode()
    {
        // Generamos el código completo de 4 dígitos
        char[] codeDigits = generatedCode.ToCharArray();

        // Creamos una lista para representar el código que se mostrará en el input (con _ donde están los libros)
        List<char> inputCode = new List<char>();

        // Lista de números que han sido asignados a los libros
        List<char> bookNumbers = new List<char>();

        // Asignamos los números de los libros a las posiciones correspondientes
        for (int i = 0; i < bookSpawns.Length; i++)
        {
            if (i < codeDigits.Length)
            {
                bookNumbers.Add(codeDigits[i]); // Añadimos el número al libro
            }
        }

        // Ahora generamos el código para mostrar en el input (con _ donde están los libros)
        for (int i = 0; i < codeDigits.Length; i++)
        {
            if (bookNumbers.Contains(codeDigits[i]))
            {
                // Si el número está en los libros, lo reemplazamos con un _
                inputCode.Add(' ');
            }
            else
            {
                // Si el número no está en los libros, lo dejamos tal como está
                inputCode.Add(codeDigits[i]);
            }
        }

        // Convertimos la lista de caracteres en una cadena para mostrar en el input
        string displayedCode = new string(inputCode.ToArray());

        // Actualizamos el inputField con el código que tiene _ donde faltan números
        GO_UIManager.Instance.SetCodeField(displayedCode);

        // Instanciamos los libros en las posiciones correspondientes
        for (int i = 0; i < bookSpawns.Length; i++)
        {
            if (i < codeDigits.Length)
            {
                Vector3 spawnPosition = bookSpawns[i].position;
                spawnPosition.y += 0.1f; // Ajustamos la altura para evitar que atraviesen el suelo

                GameObject book = Instantiate(bookPrefab, spawnPosition, Quaternion.identity);

                GO_InteractableBook bookScript = book.GetComponent<GO_InteractableBook>();
                if (bookScript != null)
                {
                    bookScript.SetNumber(codeDigits[i].ToString()); // Asignamos el número al libro
                }
                else
                {
                    Debug.LogError("El prefab del libro no tiene asignado un script GO_Book.");
                }
            }
        }
    }
    public bool ValidateCode(string userInput)
    {
        // Valida si el código ingresado por el usuario coincide con el generado.
        return userInput == generatedCode;
    }
}
