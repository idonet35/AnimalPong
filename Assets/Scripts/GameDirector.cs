using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameDirector : MonoBehaviour
{
    /// <summary>
    /// シーンステップ定義
    /// </summary>
    public enum SceneSteps : int{
        /// <summary>
        /// 最初
        /// </summary>
        Start = 0,
        /// <summary>
        /// ネクストを取り出し、再生成
        /// </summary>
        NextSwap = 1,
        /// <summary>
        /// ネクスト生成エフェクト待ち
        /// </summary>
        Ready = 2,
        /// <summary>
        /// 落下操作
        /// </summary>
        Control = 3,
        /// <summary>
        /// ゲームオーバー
        /// </summary>
        GameOver = 4
    }

    /// <summary>
    /// シーンステップ
    /// </summary>
    public SceneSteps sceneSteps;

    /// <summary>
    /// スコア
    /// </summary>
    public int score;

    /// <summary>
    /// ドロップアニム操作オブジェクト
    /// </summary>
    private GameObject _dropAnim;

    /// <summary>
    /// ドロップアニムの名前付け用カウント変数
    /// </summary>
    private int _dropCnt;

    /// <summary>
    /// 落下位置表示キャスト
    /// </summary>
    private GameObject _rayCast;

    /// <summary>
    /// 落下したアニムのリスト
    /// </summary>
    private List<GameObject> _anims;

    /// <summary>
    /// タップしたかどうか
    /// </summary>
    private bool _isPressed;

    /// <summary>
    /// ドロップする高さ
    /// </summary>
    private float _dropY = 3f;

    /// <summary>
    /// アニム消去時のエフェクトのプレハブ
    /// </summary>
    [SerializeField]
    private GameObject _padParticle;

    /// <summary>
    /// BGM用AudioSource
    /// </summary>
    [SerializeField]
    private AudioSource _audioSourceBGM;

    /// <summary>
    /// SE用AudioSource
    /// </summary>
    [SerializeField]
    private AudioSource _audioSourceSE;

    /// <summary>
    /// アニム消去時のSE
    /// </summary>
    [SerializeField]
    private AudioClip _padSE;

    [SerializeField]
    private GameObject _animParent;

    // Start is called before the first frame update
    void Start()
    {
        _dropCnt = 0;
        _anims = new List<GameObject>();
        _rayCast = GameObject.Find("RayCast");

        sceneSteps = SceneSteps.Start;
        score = 0;
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
            GameObject next = GameObject.Find("NextDirector");
            NextController nextController = next.GetComponent<NextController>();

            // ネクストを取得してドロップアニムを生成
            _dropAnim = Instantiate(nextController.GetNextAnim(), _animParent.transform);
            _dropAnim.transform.position = new Vector3(0, _dropY, 0);
            _dropAnim.transform.Rotate(0, 0, Random.Range(0, 360));
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

    /// <summary>
    /// 接触判定
    /// </summary>
    public void AnimsCollision(){
        bool isCollision = false;
        GameObject source = null;
        GameObject target = null;

        // 全アニムを検索
        foreach(var anim in _anims){
            // 衝突先を保存していれば、衝突先を調べる
            if(anim.GetComponent<AnimController>().TryGetCollision(out target)){
                // 衝突先が自分と衝突して入れば衝突判定
                if(target.GetComponent<AnimController>().TryGetCollision(out source)){
                    if(source == anim){
                        isCollision = true;
                        break;
                    }
                }
            }
        }

        // 接触が認められた場合
        if(isCollision){
            // 進化アニムを取得
            GameObject composedPref;

            // 進化後があれば合成アニムを生成
            if(GameObject.Find("AnimManager").GetComponent<AnimManager>().TryMakeConposedAnim(source, out composedPref)){
                GameObject composedAnim = Instantiate(composedPref, _animParent.transform);
                composedAnim.transform.position = (source.transform.position + target.transform.position) / 2;
                composedAnim.GetComponent<Rigidbody2D>().simulated = true;
                composedAnim.name = $"dropAnim_{_dropCnt}";
                _dropCnt++;

                // アニムリストに追加
                _anims.Add(composedAnim);
            }

            // スコアアップ
            score += GameObject.Find("AnimManager").GetComponent<AnimManager>().GetAnimScore(source);

            // スコア描画
            GameObject.Find("ScoreValue").GetComponent<TextMeshProUGUI>().SetText(score.ToString());

            // エフェクトを表示
            GameObject padPartical = Instantiate(_padParticle, _animParent.transform);
            padPartical.transform.position = (source.transform.position + target.transform.position) / 2;

            // 効果音を鳴らす
            _audioSourceSE.PlayOneShot(_padSE);

            // 接触アニムを削除
            _anims.Remove(source);
            _anims.Remove(target);
            Destroy(source);
            Destroy(target);
        }
    }
}
