using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private int _score;

    private int _nextAnim;

    private int _dropCnt;

    // 現在のシーン状況
    public SceneSteps sceneSteps;

    // シーン状況
    public enum SceneSteps : int{
        Start = 0,
        NextSwap = 1,
        Ready = 2,
        Control = 3,
        GameOver = 4
    }

    /// <summary>
    /// ドロップアニム
    /// </summary>
    private GameObject _dropAnim;

    /// <summary>
    /// 落下位置表示キャスト
    /// </summary>
    private GameObject _rayCast;

    [SerializeField]
    private List<GameObject> _anims;

    private bool _isPressed;

    [SerializeField]
    private float _dropY = 3f;

    // Start is called before the first frame update
    void Start()
    {
        _score = 0;
        _dropCnt = 0;
        _anims = new List<GameObject>();
        _rayCast = GameObject.Find("RayCast");
        sceneSteps = SceneSteps.Start;
    }

    // Update is called once per frame
    void Update()
    {
        // 最初に何秒かゲームスタートを見せてから開始
        if(sceneSteps == SceneSteps.Start){
            sceneSteps = SceneSteps.NextSwap;
        }

        // Next入れ替え
        else if(sceneSteps == SceneSteps.NextSwap){
            GameObject next = GameObject.Find("Next");
            NextController nextController = next.GetComponent<NextController>();

            // ネクストを取得
            _dropAnim = Instantiate(nextController.GetNextAnim());
            _dropAnim.transform.position = new Vector3(0, _dropY, 0);
            _dropAnim.GetComponent<Rigidbody2D>().simulated = false;
            _dropAnim.name = $"dropAnim_{_dropCnt}";
            _dropCnt++;

            // NEXTを生成
            nextController.NextGenerate();

            // 次のステップへ
            sceneSteps = SceneSteps.Ready;
        }

        // ドロップが表示されるまで待ち
        else if(sceneSteps == SceneSteps.Ready){
            if(_dropAnim.GetComponent<AnimController>().IsEnable()){
                // タップフラグ初期化
                _isPressed = false;

                // 落下位置表示
                _rayCast.GetComponent<SpriteRenderer>().enabled = true;
                Vector3 rayPos = _rayCast.transform.position;
                rayPos.x = 0f;
                _rayCast.transform.position = rayPos;

                // 次のステップへ
                sceneSteps = SceneSteps.Control;
            }
        }

        // 落下位置操作
        else if(sceneSteps == SceneSteps.Control){
            // タップ位置取得
            Vector3 mousePos = Input.mousePosition;
            // スワイプを検知
            if(Input.GetMouseButton(0)){
                // タップ位置の座標返還
                Camera camera = Camera.main;
                Vector3 worldPos = camera.ScreenToWorldPoint(mousePos);
                float xPos = worldPos.x;

                // ドロップアニムのサイズを取得
                float xSize = _dropAnim.transform.lossyScale.x / 2f;

                // 移動範囲の制限
                if(xPos <= -1.95f + xSize) xPos = -1.95f + xSize;
                if(xPos >= 1.95f - xSize) xPos = 1.95f - xSize;

                // 落下位置の移動
                _dropAnim.transform.position = new Vector3(xPos, _dropY, 0);
                Vector3 rayPos = _rayCast.transform.position;
                rayPos.x = xPos;
                _rayCast.transform.position = rayPos;
                _isPressed = true;
            }
            // 離すと落下
            else if(_isPressed){
                // 落下位置非表示
                _rayCast.GetComponent<SpriteRenderer>().enabled = false;

                // 物理シミュレーション開始
                _dropAnim.GetComponent<Rigidbody2D>().simulated = true;

                // アニムリストに追加
                _anims.Add(_dropAnim);

                // 次のステップへ
                sceneSteps = SceneSteps.NextSwap;
            }
        }

        // 接触判定
        AnimsCollision();
    }

    public void AnimsCollision(){
        bool isCollision = false;
        GameObject source = null;
        GameObject target = null;

        foreach(var anim in _anims){
            if(anim.GetComponent<AnimController>().TryGetCollision(out target)){
                if(target.GetComponent<AnimController>().TryGetCollision(out source)){
                    if(source == anim){
                        Debug.Log("destory");
                        isCollision = true;
                        break;
                    }
                }
            }
        }

        if(isCollision){
            // 進化アニムを取得
            GameObject composedPref;
            if(GameObject.Find("Next").GetComponent<NextController>().TryGetNextAnim(source.tag, out composedPref)){
                GameObject composedAnim = Instantiate(composedPref);
                composedAnim.transform.position = (source.transform.position + target.transform.position) / 2;
                composedAnim.GetComponent<Rigidbody2D>().simulated = true;
                composedAnim.name = $"dropAnim_{_dropCnt}";
                _dropCnt++;

                // アニムリストに追加
                _anims.Add(composedAnim);
            }

            // 接触アニムを削除
            _anims.Remove(source);
            _anims.Remove(target);
            Destroy(source);
            Destroy(target);
        }
    }
}
