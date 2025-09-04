using System.Collections;
using UnityEngine;

public class SkillFeedbackComponent : MonoBehaviour
{
    private Transform _playerTransform = null;

    [SerializeField]
    private DamagerData _data = null;

    [SerializeField]
    private SpriteRenderer _spriteRenderer = null;

    [SerializeField]
    private Material _material = null;

    private Coroutine _attackFeedbackCoroutine = null;
    private float _ratio = 0f;
    private float _duration = 0f;

    private void Awake()
    {
        _playerTransform = FindFirstObjectByType<PlayerControllerComponent>().transform;
    }

    private void Start()
    {
        _duration = _data.Cooldown;
        SetAttackRange(_data.AttackRadius);
        _ratio = 0f;
    }

    public bool SetAttackRange(float range)
    {
        if (_spriteRenderer == null || range <= 0)
            return false;

        transform.localScale = new Vector3(range * 2, range * 2, 1);
        return true;
    }

    public bool PlayFeedback()
    {
        if (_spriteRenderer == null)
            return false;

        //if (_attackFeedbackCoroutine != null)
        //{
        //    StopCoroutine(_attackFeedbackCoroutine);
        //}

       /* _attackFeedbackCoroutine =*/ StartCoroutine(AttackFeedback());
        return true;
    }

    private IEnumerator AttackFeedback()
    {
        _spriteRenderer.enabled = true;

        float progress = 0f;
        while (_ratio < 1)
        {
            progress += Time.deltaTime;
            _ratio = progress / _duration;
            _material.SetFloat("_Ratio", _ratio);
            yield return null;
        }

        _ratio = 0f;
        _spriteRenderer.enabled = false;
        _attackFeedbackCoroutine = null;
    }
}