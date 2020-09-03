using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using VRM;


public class ViewerController : MonoBehaviour
{
    public GameObject player;
    public GameObject DisObject;//距離のテキストオブジェクト
    public GameObject head;
    public float s=0.1f;//遷移時間を指定
    public bool FaceInBinary;//true=二値遷移 false=なめらか
    private VRMBlendShapeProxy proxy;
    private BlendShapeKey currentKey;//変更後表情
    private BlendShapeKey lastKey;//変更前の表情
    private VRMLookAtBoneApplyer eyeScript;//視線制御スクリプト

    

    [HideInInspector]public float distance;//距離
    private bool JoyF;//喜びが有効化されているか
    private bool ConfuseF;//困惑が有効化されているか
    private bool DisgustF;//不快が有効化されているか
    private bool NeutralF;//中性が有効化されているか
    private bool flag = false;//遷移が既に実行されているか(現状中性喜び分岐のみで使用)
    private bool GlanceF=true;//視線制御フラグ

    public float getDistance() { return distance; }//他スクリプトで距離を取得したいとき

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);//乱数シード
        eyeScript = GetComponent<VRMLookAtBoneApplyer>();
        /*体を動かす案1(これで動いた)(wakwakと相性悪い？)
        var animator = GetComponent<Animator>();
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        head.Rotate(new Vector3(0,5.0f, 0));
        */
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (proxy == null)
        {
            proxy = GetComponent<VRMBlendShapeProxy>();//proxy読み込み
      
            //////////////////////////////////滑らか遷移初期設定////////////////////////////////
            //初期位置を計算してフラグを書き換える
            Vector3 playerPos = new Vector3(player.transform.position.x, 0, player.transform.position.z);
            distance = Vector3.Distance(transform.position, playerPos);//プレイヤーとの距離を計算

            if (0.75f > distance)//0.75m以下
            {
                DisgustF = true;
                proxy.ImmediatelySetValue("Disgust", 1.0f);
                

            }
            else if (1.5f > distance)//1.5m以下
            {
                ConfuseF = true;
                proxy.ImmediatelySetValue("Confuse", 1.0f);

            }
            else if (distance > 1.5f)//1.5mを超える
            {
                JoyF = true;
                proxy.ImmediatelySetValue("Joy", 1.0f);
               

                
            }
            /////////////////////ここまで滑らか遷移用初期設定//////////////////////////
        }
        else
        {
            
            Vector3 playerPos = new Vector3(player.transform.position.x, 0, player.transform.position.z);
            distance= Vector3.Distance(transform.position, playerPos);//プレイヤーとの距離を計算
            
            DistanceText(distance);//距離をUIに表示する
            if (FaceInBinary)
            {
                BinaryInteraction(distance); //二値インタラクション
            }
            else
            {
                StartCoroutine("SmoothFace");//滑らか遷移関数
            }
            
            
            
            StartCoroutine("Glance");//視線制御
            
         


        }
    }

    void BinaryInteraction(float distance)   //距離によるインタラクション、出力を確実にするための関数
    {
     

        if (0.75f > distance)//0.75m以下
        {

            proxy.AccumulateValue("Confuse", 0f);
            proxy.AccumulateValue("Disgust", 1.0f);
            proxy.Apply();
        }
        else if (1.5f > distance)//1.5m以下
        {

            proxy.AccumulateValue("Disgust", 0f);
            proxy.AccumulateValue("Joy", 0f);
            proxy.AccumulateValue("Confuse", 1.0f);
            proxy.Apply();
        }
        else//1.5mを超える
        {
            if (JoyF)
            {
                proxy.AccumulateValue("Confuse", 0f);
                proxy.AccumulateValue("Neutral", 0.0f);
                proxy.AccumulateValue("Joy", 1.0f);
                proxy.Apply();
            }
            else if (NeutralF)
            {
                proxy.AccumulateValue("Confuse", 0f);
                proxy.AccumulateValue("Neutral", 1.0f);
                proxy.AccumulateValue("Joy", 0.0f);
                proxy.Apply();
            }
        }
    }


    void DistanceText(float distance)//距離をUIに表示する
    {
        Text DisText = DisObject.GetComponent<Text>();
        DisText.text = "Distance " + distance+"m";
    }




    IEnumerator SmoothFace()//滑らかな表情遷移
    {
        float changeT = 0;//現段階の遷移時間
        float lerpTA = 0f;//0~1までの割合の蓄積変数/表情が出てくる側
        float lerpTB = 1.0f;//表情が消えていく側

        
        if (distance > 1.5f)//1.5mを超える
        {
            
            if (ConfuseF)//困惑状態
            {
                
                ConfuseF = false;
                NeutralF = true;
                JoyF = false;//困惑からは喜びへ遷移（changeFaceで即中性へ遷移）
                flag = true;

                float NJInterval = Random.Range(3, 7);//次回喜び・中性分岐乱数が発生する間隔
                float seconds = 0f;//秒数

                while (s >= changeT)
                {
                    faceCal(new BlendShapeKey("Confuse"), new BlendShapeKey("Neutral"),
                            ref changeT, ref lerpTA, ref lerpTB);

                    yield return null;
                }
                BinaryInteraction(distance);//確実に表情を消失、出力するため

                while (NJInterval > seconds && (JoyF || NeutralF))
                {

                    seconds += Time.deltaTime;
                    yield return null;
                }
                flag = false;
            }
            else if(!flag)//喜びか中性か
            {
               
                flag = true;
                FaceChange();
                float NJInterval = Random.Range(3, 7);//次回喜び・中性分岐乱数が発生する間隔
                float seconds=0f;//秒数
                while (s >= changeT)
                {
                    faceCal(lastKey,currentKey,
                            ref changeT, ref lerpTA, ref lerpTB);
                    
                    yield return null;
                }
                BinaryInteraction(distance);//確実に表情を消失、出力するため
                while (NJInterval > seconds&&(JoyF||NeutralF))
                {
                    
                    seconds += Time.deltaTime;
                    yield return null;
                }
                
                flag = false;
            }
          
        }


        else if (1.5f >= distance&&0.75<distance)//1.5m以下かつ0.75より遠い
        {
            
            if (JoyF||NeutralF)//喜びか中性が有効の時
            {

                ConfuseF = true;
                JoyF = false;
                NeutralF = false;

                while (s > changeT)
                {
                    faceCal(currentKey, new BlendShapeKey("Confuse"),
                        ref changeT, ref lerpTA, ref lerpTB);
                    yield return null;
                }
             
                BinaryInteraction(distance);//確実に表情を消失、出力するため
            }
            else if (DisgustF)//不快が有効の時
            {

                ConfuseF = true;
                DisgustF = false;
                while (s > changeT)
                {
                    faceCal(new BlendShapeKey("Disgust"), new BlendShapeKey("Confuse"),
                         ref changeT, ref lerpTA, ref lerpTB);
                    yield return null;
                }
                BinaryInteraction(distance);//確実に表情を消失、出力するため
            }
     
        }
        else if (0.75f >=distance && ConfuseF)//0.75m以下かつ困惑有効
        {

            DisgustF = true;
            ConfuseF = false;

            while (s > changeT)
            {

                faceCal(new BlendShapeKey("Confuse"), new BlendShapeKey("Disgust"),
                         ref changeT, ref lerpTA, ref lerpTB);
                yield return null;
            }
            BinaryInteraction(distance);//確実に表情を消失、出力するため

        }
       

    } 
    
    void faceCal(BlendShapeKey AKey,BlendShapeKey BKey,ref float changeT,
                 ref float lerpTA, ref float lerpTB)//滑らか遷移計算部分
    {
        proxy.AccumulateValue(AKey, Mathf.Lerp(0, 1.0f, lerpTB));//消失する側
        proxy.AccumulateValue(BKey, Mathf.Lerp(0, 1.0f, lerpTA));//現れる側
        proxy.Apply();

        float ratio = Time.deltaTime / s;//割合を求める
        lerpTA += ratio;//蓄積
        lerpTB -= ratio;//減らす
        changeT += Time.deltaTime;//秒数蓄積


    }

    void FaceChange()//喜びか中性かを決定する
    {
              
        if (NeutralF)//喜びが出る
        {
            JoyF = true;
            NeutralF = false;
            lastKey = new BlendShapeKey("Neutral");//現在表情
            currentKey = new BlendShapeKey("Joy");//遷移後

        }
        else if(JoyF)//中性表情が出る
        {
            
            JoyF = false;
            NeutralF = true;
            lastKey = new BlendShapeKey("Joy");
            currentKey = new BlendShapeKey("neutral");

        }
        

    }


  IEnumerator Glance()//視線制御
    {
        float Interval = Random.Range(2.0f,5.0f);
        float seconds = 0;

       
            
            if (1.5 < distance)
            {
                eyeScript.HorizontalOuter = new CurveMapper(90.0f, 0.0f);//視線制御停止
                eyeScript.HorizontalInner = new CurveMapper(90.0f, 0.0f);
                eyeScript.VerticalDown = new CurveMapper(90.0f, 0.0f);
                eyeScript.VerticalUp = new CurveMapper(90.0f, 0.0f);

            }
            else if (1.5f >= distance && distance > 0.75f&& GlanceF)
            {
                GlanceF = false;
                if (eyeScript.HorizontalOuter.CurveYRangeDegree > 0.0f)
                {
                    eyeScript.HorizontalOuter = new CurveMapper(90.0f, 0.0f);//視線制御停止
                    eyeScript.HorizontalInner = new CurveMapper(90.0f, 0.0f);
                    eyeScript.VerticalDown = new CurveMapper(90.0f, 0.0f);
                    eyeScript.VerticalUp = new CurveMapper(90.0f, 0.0f);

            }
                else if (eyeScript.HorizontalOuter.CurveYRangeDegree == 0.0f)
                {
                    eyeScript.HorizontalOuter = new CurveMapper(90.0f, 20.0f);//視線制御
                    eyeScript.HorizontalInner = new CurveMapper(90.0f, 10.0f);
                    eyeScript.VerticalDown = new CurveMapper(90.0f, 10.0f);
                    eyeScript.VerticalUp = new CurveMapper(90.0f, 10.0f);

            }

                while (seconds < Interval)
                {
                    seconds += Time.deltaTime;
                    yield return null;

                }
                GlanceF = true;
        }
            else if (distance <= 0.75)
            {
                eyeScript.HorizontalOuter = new CurveMapper(90.0f, 20.0f);//視線制御
                eyeScript.HorizontalInner = new CurveMapper(90.0f, 10.0f);
                eyeScript.VerticalDown = new CurveMapper(90.0f, 10.0f);
                eyeScript.VerticalUp = new CurveMapper(90.0f, 10.0f);

        }

            
           
            

        
    }
  


}
