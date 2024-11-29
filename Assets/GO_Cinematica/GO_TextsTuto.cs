using System.Collections;
using TMPro;
using UnityEngine;

public class GO_TextsTuto : MonoBehaviour
{
    [Header("Configuraci�n de Textos")]
    [SerializeField] private string[] Tutotexts;  // Lista de textos a mostrar
    [SerializeField] private TMP_Text Tutotext;   // Referencia al componente TMP_Text
    [SerializeField] private float delayEntreCaracteres = 0.05f;  // Tiempo entre caracteres

    private int index = 0;  // �ndice del texto actual
    private bool escribiendo = false;  // Verifica si se est� escribiendo un texto

    // Funci�n para alternar entre textos
    public void ToggleTexts()
    {
        if (!escribiendo && index < Tutotexts.Length) // Si no est� escribiendo y hay textos restantes
        {
            StartCoroutine(EscribirTexto(Tutotexts[index])); // Escribir el texto actual
            index++;
        }
        else
        {
            Debug.Log("No hay m�s textos para mostrar o el texto est� en proceso de escritura.");
        }
    }

    // Corrutina para escribir el texto car�cter por car�cter
    private IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        Tutotext.text = "";  // Vaciar el texto antes de empezar

        foreach (char letra in texto)
        {
            Tutotext.text += letra;  // A�adir letra por letra
            yield return new WaitForSeconds(delayEntreCaracteres);  // Esperar el tiempo configurado
        }

        escribiendo = false;  // Indicar que ha terminado de escribir
    }
}
    