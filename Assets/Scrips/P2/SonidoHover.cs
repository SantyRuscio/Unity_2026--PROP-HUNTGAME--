using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SonidoHover : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("Configuración de Sonidos")]
    public AudioClip clipHover;
    public AudioClip clipClick;

    [Header("Ajustes")]
    [Range(0f, 1f)] public float volumenDeEfectos = 0.5f; 

    private AudioSource _audioSource;
    private Button _boton;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _boton = GetComponent<Button>();
        _audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_boton != null && !_boton.interactable) return;

        if (clipHover != null)
        {
            _audioSource.PlayOneShot(clipHover, volumenDeEfectos);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_boton != null && !_boton.interactable) return;

        if (clipClick != null)
        {
            _audioSource.PlayOneShot(clipClick, volumenDeEfectos);
        }
    }
}