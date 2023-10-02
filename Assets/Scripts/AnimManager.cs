using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimManager : MonoBehaviour
{
    /// <summary>
    /// アニムのプレハブ
    /// </summary>
    [SerializeField]
    private GameObject[] _animPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// no番目のプレハブを返す
    /// </summary>
    /// <param name="no">0からスタートする番号</param>
    /// <returns></returns>
    public GameObject GetAnimFromNo(int no){
        if(no < _animPrefabs.Length && no >= 0){
            return _animPrefabs[no];
        }

        return null;
    }

    /// <summary>
    /// アニム合成のスコア
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public int GetAnimScore(GameObject anim){
        string tag = anim.tag;

        int retScore = 0;
        for(int i = 0; i < _animPrefabs.Length - 1; i++){
            retScore += i + 1;

            if(_animPrefabs[i].tag == tag){
                break;
            }
        }
        return retScore;
    }

    /// <summary>
    /// 合成後のアニムを出力する
    /// </summary>
    /// <param name="beforeAnim">合成前のアニムの入力</param>
    /// <param name="afterAnim">合成後のアニムの出力</param>
    /// <returns>合成の可否</returns>
    public bool TryMakeConposedAnim(GameObject beforeAnim, out GameObject afterAnim){
        string tag = beforeAnim.tag;
        afterAnim = null;

        for(int i = 0; i < _animPrefabs.Length - 1; i++){
            if(_animPrefabs[i].tag == tag){
                afterAnim = _animPrefabs[i + 1];
                return true;
            }
        }

        return false;
    }
}
