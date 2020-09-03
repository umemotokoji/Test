using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

public class FaceDirector : MonoBehaviour
{
    public GameObject model;
    private VRMBlendShapeProxy proxy;
    private BlendShapeKey a = new BlendShapeKey("Joy");
    private BlendShapeKey currentKey;//現在の表情
   
    

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (proxy == null)
        {
            proxy = model.GetComponent<VRMBlendShapeProxy>();//BlendShape読み込み
            proxy.ImmediatelySetValue("Confuse", 1.0f);
            currentKey = new BlendShapeKey("Confuse");
        }
        else
        {
           

            if (Input.GetKeyDown("1"))
            {
                proxy.ImmediatelySetValue("Disgust", 0f);
                proxy.ImmediatelySetValue("Confuse", 0f);
                proxy.ImmediatelySetValue(a, 1f);//表情の設定
               

            }


            else if (Input.GetKeyDown("2"))
            {
                proxy.ImmediatelySetValue("Disgust", 0f);
                proxy.ImmediatelySetValue(a, 0f);
                proxy.ImmediatelySetValue("Confuse", 1f);//表情の設定

            }

            else if (Input.GetKeyDown("3"))
            {
            
                proxy.ImmediatelySetValue("Confuse", 0f);
                proxy.ImmediatelySetValue(a, 0f);
                proxy.ImmediatelySetValue("Disgust", 1f);//表情の設定
               
            }

            else if (Input.GetKeyDown("0"))
            {

                proxy.ImmediatelySetValue("Confuse", 0f);
                proxy.ImmediatelySetValue(BlendShapePreset.Joy, 0f);
                proxy.ImmediatelySetValue("Disgust", 0f);


            }
            else if (Input.GetKeyDown("9"))//不快
            {
                StartCoroutine(setFace(currentKey, new BlendShapeKey("Disgust")));
                currentKey = new BlendShapeKey("Disgust");

            }
            else if (Input.GetKeyDown("8"))//困惑の顔
            {
                StartCoroutine(setFace(currentKey, new BlendShapeKey("Confuse")));
                currentKey = new BlendShapeKey("Confuse");

            }



        }
    }

    IEnumerator setFace(BlendShapeKey A,BlendShapeKey B)
    {
        float changeT = 0;//現段階の遷移時間
        float lerpTA = 0f;//0~1までの割合の蓄積変数/表情が出てくる側
        float lerpTB = 1.0f;//表情が消えていく側

        while (changeT < 0.5)
        {

            proxy.ImmediatelySetValue(A, Mathf.Lerp(0, 1.0f, lerpTB));//無効化する
            proxy.ImmediatelySetValue(B, Mathf.Lerp(0, 1.0f, lerpTA));//有効化する


            float ratio = Time.deltaTime / 0.5f;//割合を求める
            lerpTA += ratio;//蓄積
            lerpTB -= ratio;//減らす
           // Debug.Log("LerpTB=" + lerpTB);
            Debug.Log("LerpTA=" + lerpTA);
            changeT += Time.deltaTime;//秒数蓄積
            yield return null;
        }
    }

}
