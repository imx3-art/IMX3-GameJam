using System.Collections;
using TMPro;
using UnityEngine;

public class GO_TextsTuto : MonoBehaviour
{
    [Header("Configuración de Textos")]
    [SerializeField] private string[] Tutotexts;  // Lista de textos a mostrar
    [SerializeField] private TMP_Text Tutotext;   // Referencia al componente TMP_Text
    [SerializeField] private float delayEntreCaracteres = 0.05f;  // Tiempo entre caracteres

    private int index = 0;  // Índice del texto actual
    private bool escribiendo = false;  // Verifica si se está escribiendo un texto

    // Función para alternar entre textos
    public void ToggleTexts()
    {
        if (!escribiendo && index < Tutotexts.Length) // Si no está escribiendo y hay textos restantes
        {
            StartCoroutine(EscribirTexto(Tutotexts[index])); // Escribir el texto actual
            index++;
        }
        else
        {
            Debug.Log("No hay más textos para mostrar o el texto está en proceso de escritura.");
        }
    }

    // Corrutina para escribir el texto carácter por carácter
    private IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        Tutotext.text = "";  // Vaciar el texto antes de empezar

        foreach (char letra in texto)
        {
            Tutotext.text += letra;  // Añadir letra por letra
            yield return new WaitForSeconds(delayEntreCaracteres);  // Esperar el tiempo configurado
        }

        escribiendo = false;  // Indicar que ha terminado de escribir
    }
}
    