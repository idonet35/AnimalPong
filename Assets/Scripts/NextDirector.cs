using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextController : MonoBehaviour
{
    // 各要素の重みリスト
    [SerializeField]
    private float[] _weights;

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
        // Nextに選択されているプレハブを返す
        return selectedAnim;
    }

    public int NextGenerate(){
        // Next生成
        int sel = Choose();

        // Nextのプレハブを取得
        selectedAnim = GameObject.Find("AnimManager").GetComponent<AnimManager>().GetAnimFromNo(sel);

        // Nextのイメージ変更
        GameObject obj = GameObject.Find("NextImage");
        var compo = obj.GetComponent<Image>();
        compo.sprite = selectedAnim.GetComponent<SpriteRenderer>().sprite;
        return sel;
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