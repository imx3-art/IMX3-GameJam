using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_CodeManager : NetworkBehaviour
{
    public static GO_CodeManager Instance; // Singleton para acceso global.

    [Networked] private string generatedCode { get => default; set { } } // C�digo aleatorio generado.
    public Transform[] bookSpawns; // Lugares donde aparecer�n los libros.
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
        // Genera un c�digo de 4 d�gitos como string.
        generatedCode = Random.Range(1000, 9999).ToString();
        Debug.Log($"C�digo generado: {generatedCode}");

        DistributeCode();
    }

    public void DistributeCode()
    {
        // Generamos el c�digo completo de 4 d�gitos
        char[] codeDigits = generatedCode.ToCharArray();

        // Creamos una lista para representar el c�digo que se mostrar� en el input (con _ donde est�n los libros)
        List<char> inputCode = new List<char>();

        // Lista de n�meros que han sido asignados a los libros
        List<char> bookNumbers = new List<char>();

        // Asignamos los n�meros de los libros a las posiciones correspondientes
        for (int i = 0; i < bookSpawns.Length; i++)
        {
            if (i < codeDigits.Length)
            {
                bookNumbers.Add(codeDigits[i]); // A�adimos el n�mero al libro
            }
        }

        // Ahora generamos el c�digo para mostrar en el input (con _ donde est�n los libros)
        for (int i = 0; i < codeDigits.Length; i++)
        {
            if (bookNumbers.Contains(codeDigits[i]))
            {
                // Si el n�mero est� en los libros, lo reemplazamos con un _
                inputCode.Add(' ');
            }
            else
            {
                // Si el n�mero no est� en los libros, lo dejamos tal como est�
                inputCode.Add(codeDigits[i]);
            }
        }

        // Convertimos la lista de caracteres en una cadena para mostrar en el input
        string displayedCode = new string(inputCode.ToArray());

        // Actualizamos el inputField con el c�digo que tiene _ donde faltan n�meros
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
                    bookScript.SetNumber(codeDigits[i].ToString()); // Asignamos el n�mero al libro
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
        // Valida si el c�digo ingresado por el usuario coincide con el generado.
        return userInput == generatedCode;
    }
}
