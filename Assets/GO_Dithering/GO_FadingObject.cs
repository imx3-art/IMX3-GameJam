using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GO_FadingObject : MonoBehaviour, IEquatable<GO_FadingObject>
{
    public List<Renderer> Renderers = new List<Renderer>();
    public Vector3 Position;
    public List<Material> Materials = new List<Material>();
    [HideInInspector]
    public float InitialAlpha;

    private void Awake()
    {
        Position = transform.position;

        if (Renderers.Count == 0)
        {
            Renderers.AddRange(GetComponentsInChildren<Renderer>());
        }
        foreach(Renderer renderer in Renderers)
        {
            Materials.AddRange(renderer.materials);
        }

        // Asegúrate de que la lista de materiales no esté vacía
        if (Materials.Count > 0)
        {
            // Verifica si el material tiene la propiedad "_Color"
            if (Materials[0].HasProperty("_Color"))
            {
                // Obtén el alpha inicial del material
                InitialAlpha = Materials[0].color.a;

                // Si el alpha inicial es cero, lo establecemos a 1 por defecto
                if (InitialAlpha == 0f)
                {
                    InitialAlpha = 1f;
                }
            }
            else
            {
                // Si el material no tiene la propiedad "_Color", establecemos el alpha inicial a 1
                InitialAlpha = 1f;
            }
        }
        else
        {
            Debug.LogError("No se encontraron materiales en el GO_FadingObject.");
            InitialAlpha = 1f; // Valor por defecto si no hay materiales
        }
    }

    public bool Equals(GO_FadingObject other)
    {
        return Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}
