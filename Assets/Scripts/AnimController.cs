using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimController : MonoBehaviour
{
    private float _originalScale;

    private float _zoom;

    private bool _isCollisioned;

    private GameObject _collisionObject;
    
    // Start is called before the first frame update
    void Start()
    {
        _originalScale = transform.localScale.x;
        _zoom = 0f;
        _isCollisioned = false;
        _collisionObject = null;
        transform.localScale = new Vector3(0, 0, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if(_zoom < 1f){
            _zoom += Time.deltaTime * 2;
            if(_zoom > 1f) _zoom = 1f;
            float s = _originalScale * _zoom;
            transform.localScale = new Vector3(s, s, 1f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision){
        if(gameObject.tag == collision.gameObject.tag){
            _collisionObject = collision.gameObject;
        }
        _isCollisioned = true;
    }

    public bool TryGetCollision(out GameObject collisionObject){
        collisionObject = null;

        if(_collisionObject!=null){
            collisionObject = _collisionObject;
            return true;
        }
        return false;
    }

    public bool IsEnable(){
        return true ? _zoom == 1f : false;
    }

    public bool IsCollisioned(){
        return _isCollisioned;
    }
}
