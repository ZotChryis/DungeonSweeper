using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : SingletonMonoBehaviour<TransitionManager>
{
    public float FadeDuration = 1f;
    
    private int FadeAmount = Shader.PropertyToID("_FadeAmount");
    private int UseShutters =  Shader.PropertyToID("_UseShutters");
    private int UseGoop =  Shader.PropertyToID("_UseGoop");

    private int? LastTransition;
    
    private Image Image;
    private Material Material;

    public enum TransitionType
    {
        Shutters,
        Goop,
    }
    
    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
            
            Image = GetComponent<Image>();

            // Make an instance of the material so we don't update the serialized one
            Material = new Material(Image.material);
            Image.material = Material;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void DoTransition(TransitionType transition, Action onTransitionIn)
    {
        StartCoroutine(DoTransitionHelper(transition, onTransitionIn));
    }

    private IEnumerator DoTransitionHelper(TransitionType transition, Action onTransitionIn)
    {
        TransitionOut(transition);
        yield return new WaitForSeconds(FadeDuration);
        onTransitionIn?.Invoke();
        TransitionIn(transition);
    }

    public void TransitionOut(TransitionType transition)
    {
        SetTransitionType(transition);
        
        Material.SetFloat(FadeAmount, 0f);
        Material
            .DOFloat(1f, FadeAmount, FadeDuration)
            .SetEase(Ease.InOutSine);
    }
    
    public void TransitionIn(TransitionType transition)
    {
        SetTransitionType(transition);
        
        Material.SetFloat(FadeAmount, 1f);
        Material
            .DOFloat(0f, FadeAmount, FadeDuration)
            .SetEase(Ease.InOutSine);
    }

    private void SetTransitionType(TransitionType transition)
    {
        if (LastTransition.HasValue)
        {
            Material.SetFloat(LastTransition.Value, 0.0f);
        }
        
        switch (transition)
        {
            case TransitionType.Shutters:
                Material.SetFloat(UseShutters, 1.0f);
                LastTransition = UseShutters;
                break;
            
            case TransitionType.Goop:
                Material.SetFloat(UseGoop, 1.0f);
                LastTransition = UseGoop;
                break;
        }
    }
}
