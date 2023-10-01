using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextController : MonoBehaviour
{
    // 各要素の重みリスト
    [SerializeField]
    private float[] _weights;

    [SerializeField]
    private GameObject[] _anims;

    [SerializeField]
    private GameObject _nextAnim;

    private GameObject selectedAnim;
    
    // 重みの総和（初期化時に計算される）
    private float _totalWeight;

    // Start is called before the first frame update
    void Start()
    {
        _totalWeight = 0f;

        // 重みの総和計算
        for (var i = 0; i < _weights.Length; i++)
        {
            _totalWeight += _weights[i];
        }

        // 最初の設定
        NextGenerate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetNextAnim(){
        return selectedAnim;
    }

    public int NextGenerate(){
        int sel = Choose();

        selectedAnim = _anims[sel];
        _nextAnim.GetComponent<SpriteRenderer>().sprite = selectedAnim.GetComponent<SpriteRenderer>().sprite;
        return sel;
    }

    public bool TryGetNextAnim(string tag, out GameObject nextAnim){
        nextAnim = null;

        for(int i = 0; i < _anims.Length - 1; i++){
            if(_anims[i].tag == tag){
                nextAnim = _anims[i + 1];
                return true;
            }
        }

        return false;
    }

    private int Choose()
    {
        // 0～重みの総和の範囲の乱数値取得
        var randomPoint = Random.Range(0, _totalWeight);

        // 乱数値が属する要素を先頭から順に選択
        var currentWeight = 0f;
        for (var i = 0; i < _weights.Length; i++)
        {
            // 現在要素までの重みの総和を求める
            currentWeight += _weights[i];

            // 乱数値が現在要素の範囲内かチェック
            if (randomPoint < currentWeight)
            {
                return i;
            }
        }

        // 乱数値が重みの総和以上なら末尾要素とする
        return _weights.Length - 1;
    }
}

public class NextFaceWeight
{
    public GameObject animPrefab;

    public float weight;
}
